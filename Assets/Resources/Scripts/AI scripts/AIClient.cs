using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

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

    [Header("Status")]
    public bool isConnected
    {
        get; set;
    }

    public string currentServerURL = "";

    private List<Message> conversationHistory = new List<Message>();

    public event Action<string> OnAIResponseReceived;
    public event Action<bool> OnConnectionStatusChanged;
    public event Action<string> OnConnectionError;
    protected override Type GetServiceType() => typeof(IAIService);

    void Start()
    {
        InitializeConversation();
        StartCoroutine(AutoDetectServer());
    }

    private void InitializeConversation()
    {
        conversationHistory.Clear();
        conversationHistory.Add(new Message
        {
            role = "system",
            content = "Тестовый запрос. Ответь кратко."
        });
    }

    private IEnumerator AutoDetectServer()
    {
        bool wasConnected = isConnected;

        foreach (string url in serverURLs)
        {
            Debug.Log($"Checking connection to: {url}");
            yield return StartCoroutine(TestConnection(url, (success) => {
                if (success)
                {
                    currentServerURL = url;
                    isConnected = true;

                    if (!wasConnected)
                    {
                        OnConnectionStatusChanged?.Invoke(true);
                    }

                    Debug.Log($"Successfully connected to: {url}");
                }
            }));

            if (isConnected) break;
            yield return new WaitForSeconds(2f);
        }

        if (!isConnected && wasConnected)
        {
            OnConnectionStatusChanged?.Invoke(false);
        }
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
            request.timeout = 3;

            yield return request.SendWebRequest();

            callback(request.result == UnityWebRequest.Result.Success);
        }
    }

    private string CleanAIResponse(string rawResponse)
    {
        if (string.IsNullOrEmpty(rawResponse))
            return rawResponse;

        // Удаляем служебные токены
        string cleaned = rawResponse;

        // Удаляем лишние пробелы
        cleaned = cleaned.Trim();

        // Если после очистки строка пустая, возвращаем исходную
        if (string.IsNullOrEmpty(cleaned))
            return rawResponse;

        return cleaned;
    }

    public void SendMessageToAI(string userMessage)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Нейросеть не подключена. Запустите LM Studio с сервером.");
            return;
        }

        StartCoroutine(SendAIRequest(userMessage));
    }

    private IEnumerator SendAIRequest(string userMessage)
    {
        conversationHistory.Add(new Message
        {
            role = "user",
            content = userMessage
        });

        AIRequest requestData = new AIRequest
        {
            messages = conversationHistory,
            temperature = 0.7,
            max_tokens = 300
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(currentServerURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка связи с нейросетью: {request.error}");

                bool wasConnected = isConnected;
                isConnected = false;

                // Уведомление об ошибке и разрыве соединения
                OnConnectionError?.Invoke(request.error);
                if (wasConnected)
                {
                    OnConnectionStatusChanged?.Invoke(false);
                }

                // Попытка переподключиться
                StartCoroutine(AutoDetectServer());
            }
            else
            {
                HandleAIResponse(request.downloadHandler.text);
            }
        }
}

    private void HandleAIResponse(string jsonResponse)
    {
        try
        {
            AIResponse response = JsonUtility.FromJson<AIResponse>(jsonResponse);

            if (response.choices != null && response.choices.Count > 0 && response.choices[0].message != null)
            {
                string rawMessage = response.choices[0].message.content;
                string cleanMessage = CleanAIResponse(rawMessage);

                conversationHistory.Add(new Message
                {
                    role = "assistant",
                    content = cleanMessage
                });

                Debug.Log($"AI Response: {cleanMessage}");
                OnAIResponseReceived?.Invoke(cleanMessage);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing AI response: {e.Message}");
        }
    }

    public void SendMessage(string message)
    {
        if (!isConnected)
        {
            Debug.LogWarning("AI not connected. Cannot send message.");
            return;
        }

        StartCoroutine(SendAIRequest(message));
    }

    public void RetryConnection()
    {
        StartCoroutine(AutoDetectServer());
    }
}
