using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using LLMUnity;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SceneLoad;
using EventBusSystem;

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
    [SerializeField] private LLMAgent llmAgent;

    [Header("Heartbeat Settings")]
    public float heartbeatInterval = 5f;
    public float requestTimeout = 14f;

    [Header("Status")]
    [SerializeField] private bool _isConnected;

    // Подключение LLM
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
    //private SceneLoadingService _loadingService;


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

    private async void Start()
    {
        if (llmAgent == null)
        {
            Debug.LogError("[AIClient] LLMAgent не назначен!");
            return;
        }
        await llmAgent.ClearHistory();
        //if (!IsConnected)
        //    await UniTask.WaitUntil(() => IsConnected);
        //_loadingService = ServiceLocator.Instance.GetService<SceneLoadingService>();
       await InitializeConnectionAsync();
    }

    public async UniTaskVoid SetSystemPrompt(string systemPrompt)
    {
        if (llmAgent == null) return;
        llmAgent.systemPrompt = systemPrompt;
        await llmAgent.ClearHistory();
    }

    public async UniTask InitializeConnectionAsync()
    {
        IsConnected = false;
        if (llmAgent != null && llmAgent.llm != null)
        {
            await llmAgent.llm.WaitUntilReady();
            IsConnected = true;
            OnConnectionStatusChanged?.Invoke(true);
            Debug.Log("[AIClient] LLM модель загружена и готова.");
        }
        else
        {
            Debug.LogError("[AIClient] LLMAgent не назначен!");
        }
    }

    public async UniTask LoadModelAsync(IProgress<float> progress = null)
    {
        if (IsConnected) return;
        // Имитация прогресса (если плагин не даёт реального)
        await InitializeConnectionAsync();
        //if (progress != null)
        //{
        //    _ = SimulateProgress(progress, loadTask);
        //}
        //await loadTask;
    }

    //private async UniTask SimulateProgress(IProgress<float> progress, UniTask loadTask)
    //{
    //    float elapsed = 0f;
    //    float estimatedDuration = 6f; 
    //    while (!loadTask.Status.IsCompleted() && elapsed < estimatedDuration)
    //    {
    //        elapsed += Time.deltaTime;
    //        progress?.Report(Mathf.Clamp01(elapsed / estimatedDuration));
    //        EventBus.RaiseEvent<ISceneLoadProgressSubscriber>(s => s.OnSceneLoadProgress(progress));
    //        await UniTask.Yield();
    //    }
    //    if (!loadTask.Status.IsCompleted())
    //        progress?.Report(1f);
    //}

    private IEnumerator AutoDetectServer()
    {
        bool wasConnected = IsConnected;
        IsConnected = false;

        if (llmAgent.llm.started)
        {
            wasConnected = true;
            OnConnectionStatusChanged?.Invoke(true);
        }
        else if (llmAgent.llm.failed)
        {
            OnConnectionStatusChanged?.Invoke(false);
            yield break;
        }
        StopHeartbeat();
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
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

    async public UniTaskVoid SendMessage(string message)
    {

        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("[AIClient] Пустое сообщение");
            return;
        }

        _isWaitingForResponse = true;
        _pendingResponse = null;
        _pendingError = null;

        StopCurrentRequest();
        string fullResponse = await llmAgent.Chat(message);
        OnChatSuccess(fullResponse);
    }

    public void OnChatSuccess(string response)
    {
        _pendingResponse = response;
        _isWaitingForResponse = false;
        string cleanMessage = CleanAIResponse(response);
        Debug.Log($"[AIClient] Ответ AI: {cleanMessage}");
        OnAIResponseReceived?.Invoke(cleanMessage);
    }

    public void BreakeMessage()
    {
        StopCurrentRequest();
        llmAgent.CancelRequests();
    }
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
        if (llmAgent != null)
            llmAgent.ClearHistory();
        Debug.Log("[AIClient] История диалога очищена");
    }

    private void OnDestroy()
    {
        llmAgent.CancelRequests();
        llmAgent.ClearHistory();
        llmAgent.llmAgent.Dispose();
        StopHeartbeat();
        StopCurrentRequest();
    }
}