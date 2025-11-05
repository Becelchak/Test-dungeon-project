using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using UnityEditor.Rendering;

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

public class AIClient : MonoBehaviour
{
    [Header("AI Connection Settings")]
    public string[] serverURLs = {
        "http://localhost:1234/v1/chat/completions",
        "http://localhost:8080/v1/chat/completions",
        "http://127.0.0.1:1234/v1/chat/completions"
    };

    [Header("Status")]
    public bool isConnected = false;
    public string currentServerURL = "";

    private List<Message> conversationHistory = new List<Message>();
    private int currentServerIndex = 0;

    public event Action<bool> OnConnectionStatusChanged;
    public event Action<string> OnConnectionError;

    [Header("Dialog")]
    [SerializeField] private TMPro.TextMeshProUGUI dialogText;


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
            Debug.Log($"Проверка подключения к: {url}");
            yield return StartCoroutine(TestConnection(url, (success) => {
                if (success)
                {
                    currentServerURL = url;
                    isConnected = true;

                    // Уведомляем об изменении статуса
                    if (!wasConnected)
                    {
                        OnConnectionStatusChanged?.Invoke(true);
                    }

                    Debug.Log($"Успешно подключено к: {url}");
                }
            }));

            if (isConnected) break;
            yield return new WaitForSeconds(1f);
        }

        if (!isConnected)
        {
            Debug.LogWarning("Не удалось подключиться к локальной нейросети");
            if (wasConnected)
            {
                OnConnectionStatusChanged?.Invoke(false);
            }
        }
    }

    private IEnumerator TestConnection(string url, System.Action<bool> callback)
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

        // Удаляем служебные токены gpt-oss-20b
        string cleaned = rawResponse;

        // Удаляем <|channel|> commentary to=assistant
        if (cleaned.StartsWith("<|channel|>commentary to=assistant"))
        {
            cleaned = cleaned.Replace("<|channel|>commentary to=assistant", "").Trim();
        }

        // Удаляем другие возможные служебные токены
        cleaned = cleaned.Replace("<|channel|>", "");
        cleaned = cleaned.Replace("commentary to=assistant", "");
        cleaned = cleaned.Replace("commentary to=user", "");

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

        //dialogText.text = userMessage;

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

                // Уведомляем об ошибке и разрыве соединения
                OnConnectionError?.Invoke(request.error);
                if (wasConnected)
                {
                    OnConnectionStatusChanged?.Invoke(false);
                }

                // Попробуем переподключиться
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
                string aiMessage = response.choices[0].message.content;
                aiMessage = CleanAIResponse(aiMessage);

                conversationHistory.Add(new Message
                {
                    role = "assistant",
                    content = aiMessage
                });

                Debug.Log($"AI: {aiMessage}");
                dialogText.text = aiMessage;
                OnAIResponseReceived?.Invoke(aiMessage);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка обработки ответа: {e.Message}");
        }
    }

    public System.Action<string> OnAIResponseReceived;

    public void RetryConnection()
    {
        StartCoroutine(AutoDetectServer());
    }
}
