using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using LLMUnity;

[System.Serializable]
public class AIRequest
{
    public List<Message> messages;
    public double temperature = 0.7;
    public int max_tokens = 500;
    public bool stream = false;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class AIResponse
{
    public string id;
    public string @object;
    public int created;
    public string model;
    public List<Choice> choices;
    public Usage usage;
    public string error;
}

[System.Serializable]
public class Choice
{
    public int index;
    public Message message;
    public string finish_reason;
}

[System.Serializable]
public class Usage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}

public class AIClient : BaseService, IAIService
{
    [Header("AI Connection Settings")]
    public string[] serverURLs = {
        "http://localhost:1234/v1/chat/completions",
        "http://localhost:8080/v1/chat/completions",
        "http://127.0.0.1:1234/v1/chat/completions"
    };
    [SerializeField] private LLMAgent llmAgent;

    [Header("Heartbeat Settings")]
    public float heartbeatInterval = 5f;
    public float requestTimeout = 14f;

    [Header("Status")]
    [SerializeField] private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                Debug.Log($"[AIClient] Состояние подключения: {(value ? "Подключено" : "Отключено")}");
            }
        }
    }

    public string currentServerURL = "";
    private List<Message> _conversationHistory = new List<Message>();
    private Coroutine _heartbeatCoroutine;
    private Coroutine _currentRequest;


    private bool _isWaitingForResponse = false;
    private string _pendingResponse = null;
    private string _pendingError = null;
    // События IAIService
    public event Action<string> OnAIResponseReceived;
    public event Action<bool> OnConnectionStatusChanged;
    public event Action<string> OnConnectionError;

    protected override Type GetServiceType() => typeof(IAIService);

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        //InitializeConversation();
        //StartCoroutine(AutoDetectServer());

        if (llmAgent == null)
        {
            Debug.LogError("[AIClient] LLMAgent не назначен!");
        }
        else
        {
            llmAgent.ClearHistory();
        }
    }

    public void SetSystemPrompt(string systemPrompt)
    {
        if (llmAgent != null)
        {
            llmAgent.systemPrompt = systemPrompt;
            // Очищаем историю, чтобы начать новый диалог
            llmAgent.ClearHistory();
        }
    }

    //private void InitializeConversation()
    //{
    //    _conversationHistory.Clear();
    //    _conversationHistory.Add(new Message
    //    {
    //        role = "system",
    //        content = "Ты актер. Отвечай на поставленные вопросы. При указании твоей роли - следуй ей."
    //    });
    //}

    private IEnumerator AutoDetectServer()
    {
        bool wasConnected = IsConnected;
        IsConnected = false;

        foreach (string url in serverURLs)
        {
            Debug.Log($"[AIClient] Проверка подключения к: {url}");
            bool connectionSuccess = false;
            yield return StartCoroutine(TestConnection(url, (success) => connectionSuccess = success));

            if (connectionSuccess)
            {
                currentServerURL = url;
                IsConnected = true;
                StartHeartbeat();

                if (!wasConnected)
                {
                    Debug.Log($"[AIClient] Успешно подключено к: {url}");
                    OnConnectionStatusChanged?.Invoke(true);
                }
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        // Если ни один сервер не ответил
        if (wasConnected)
        {
            Debug.LogWarning("[AIClient] Соединение потеряно");
            OnConnectionStatusChanged?.Invoke(false);
        }
        else
        {
            Debug.LogWarning("[AIClient] Не удалось подключиться к серверу AI");
        }

        StopHeartbeat();
    }

    private IEnumerator TestConnection(string url, Action<bool> callback)
    {
        string testJson = @"{
            ""messages"": [{""role"": ""user"", ""content"": ""Ответь 'готов'""}],
            ""max_tokens"": 10
        }";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(testJson);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            callback(success);

            if (!success)
            {
                Debug.LogWarning($"[AIClient] Тест подключения к {url} не удался: {request.error}");
            }
        }
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();
        _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
        Debug.Log("[AIClient] Запущен мониторинг соединения");
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }
    }

    private IEnumerator HeartbeatCoroutine()
    {
        while (IsConnected)
        {
            yield return new WaitForSeconds(heartbeatInterval);

            if (!IsConnected) yield break;

            yield return StartCoroutine(TestConnection(currentServerURL, (success) =>
            {
                if (!success && IsConnected)
                {
                    Debug.LogWarning("[AIClient] Heartbeat: соединение потеряно");
                    HandleConnectionLost("Сервер не отвечает");
                }
            }));
        }
    }

    private void HandleConnectionLost(string error)
    {
        bool wasConnected = IsConnected;
        IsConnected = false;
        StopHeartbeat();
        StopCurrentRequest();

        OnConnectionError?.Invoke(error);
        if (wasConnected)
        {
            OnConnectionStatusChanged?.Invoke(false);
        }
    }

    private void StopCurrentRequest()
    {
        if (_currentRequest != null)
        {
            StopCoroutine(_currentRequest);
            _currentRequest = null;
        }
    }

    async public void SendMessage(string message)
    {
        //if (!IsConnected)
        //{
        //    Debug.LogWarning("[AIClient] AI не подключен. Невозможно отправить сообщение.");
        //    OnConnectionError?.Invoke("AI не подключен");
        //    return;
        //}

        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("[AIClient] Пустое сообщение");
            return;
        }

        _isWaitingForResponse = true;
        _pendingResponse = null;
        _pendingError = null;

        StopCurrentRequest();
        string fullResponse = await llmAgent.Chat(message); // Ждем полный ответ
        OnChatSuccess(fullResponse);
        //_currentRequest = StartCoroutine(SendAIRequest(message));
    }

    private void OnChatSuccess(string response)
    {
        _pendingResponse = response;
        _isWaitingForResponse = false;
        // Обработка ответа (очистка от тегов <think> и т.п.)
        string cleanMessage = CleanAIResponse(response);
        Debug.Log($"[AIClient] Ответ AI: {cleanMessage}");
        OnAIResponseReceived?.Invoke(cleanMessage);
    }

    private void OnChatError(string error)
    {
        _pendingError = error;
        _isWaitingForResponse = false;
        Debug.LogError($"[AIClient] Ошибка при получении ответа: {error}");
        OnConnectionError?.Invoke(error);
        // Можно также попробовать переподключиться
    }

    public void BreakeMessage()
    {
        StopCurrentRequest();
        llmAgent.CancelRequests();
    }

    //private IEnumerator SendAIRequest(string userMessage)
    //{
    //    Debug.Log($"[AIClient] Отправка сообщения: {userMessage}");

    //    _conversationHistory.Add(new Message
    //    {
    //        role = "user",
    //        content = userMessage
    //    });

    //    //AIRequest requestData = new AIRequest
    //    //{
    //    //    messages = _conversationHistory,
    //    //    temperature = 0.4,
    //    //    max_tokens = 180
    //    //};

    //    //string jsonData = JsonUtility.ToJson(requestData);
    //    //byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

    //    //using (UnityWebRequest request = new UnityWebRequest(currentServerURL, "POST"))
    //    //{
    //    //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    //    request.SetRequestHeader("Content-Type", "application/json");
    //    //    request.timeout = (int)requestTimeout;

    //    //    int maxRetries = 2;
    //    //    int attempt = 0;

    //    //    while (attempt <= maxRetries)
    //    //    {
    //    //        yield return request.SendWebRequest();
    //    //        if (request.result == UnityWebRequest.Result.Success) break;
    //    //        if (request.result == UnityWebRequest.Result.ConnectionError && attempt < maxRetries)
    //    //        {
    //    //            attempt++;
    //    //            Debug.Log($"Попытка {attempt}...");
    //    //            yield return new WaitForSeconds(1f);
    //    //            continue;
    //    //        }
    //    //        break;
    //    //    }

    //    //    if (request.result != UnityWebRequest.Result.Success)
    //    //    {
    //    //        Debug.LogError($"[AIClient] Ошибка связи: {request.error}");
    //    //        HandleConnectionLost(request.error);
    //    //    }
    //    //    else
    //    //    {
    //    //        HandleAIResponse(request.downloadHandler.text);
    //    //    }
    //    //}

    //    _ = llmAgent.Chat(userMessage);
    //    yield return new WaitForSeconds(1f);

    //    _currentRequest = null;
    //}

    //private void HandleAIResponse(string jsonResponse)
    //{
    //    try
    //    {
    //        AIResponse response = JsonUtility.FromJson<AIResponse>(jsonResponse);

    //        if (response.choices != null && response.choices.Count > 0 && response.choices[0].message != null)
    //        {
    //            string rawMessage = response.choices[0].message.content;
    //            string cleanMessage = CleanAIResponse(rawMessage);

    //            _conversationHistory.Add(new Message
    //            {
    //                role = "assistant",
    //                content = cleanMessage
    //            });

    //            Debug.Log($"[AIClient] Ответ AI: {cleanMessage}");
    //            OnAIResponseReceived?.Invoke(cleanMessage);
    //        }
    //        else if (!string.IsNullOrEmpty(response.error))
    //        {
    //            Debug.LogError($"[AIClient] Ошибка AI: {response.error}");
    //            OnConnectionError?.Invoke(response.error);
    //        }
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"[AIClient] Ошибка обработки ответа: {e.Message}");
    //        OnConnectionError?.Invoke(e.Message);
    //    }
    //}

    private string CleanAIResponse(string rawResponse)
    {
        if (string.IsNullOrEmpty(rawResponse))
            return rawResponse;

        string pattern = @"<think>.*?</think>";
        string cleaned = System.Text.RegularExpressions.Regex.Replace(rawResponse, pattern, "", System.Text.RegularExpressions.RegexOptions.Singleline).Trim();
        return string.IsNullOrEmpty(cleaned) ? rawResponse : cleaned;
    }

    public void RetryConnection()
    {
        Debug.Log("[AIClient] Повторное подключение...");
        StopHeartbeat();
        StopCurrentRequest();
        StartCoroutine(AutoDetectServer());
    }

    public void ClearConversation()
    {
        //_conversationHistory.Clear();
        if (llmAgent != null)
            llmAgent.ClearHistory();
        //InitializeConversation();
        Debug.Log("[AIClient] История диалога очищена");
    }

    private void OnDestroy()
    {
        StopHeartbeat();
        StopCurrentRequest();
    }
}