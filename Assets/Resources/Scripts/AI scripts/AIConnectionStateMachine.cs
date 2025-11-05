using System;
using System.Collections;
using UnityEngine;

public enum AIConnectionState
{
    Disconnected,      // Нет подключения
    Connecting,        // В процессе подключения
    Connected,         // Успешно подключено
    Error,             // Ошибка подключения
    Reconnecting       // Автоматическое переподключение
}

public class AIConnectionStateMachine : MonoBehaviour
{
    [Header("Settings")]
    public float reconnectInterval = 5f;
    public int maxReconnectAttempts = 3;

    [Header("Debug")]
    [SerializeField] private AIConnectionState _currentState;

    // События для внешних подписчиков
    public event Action<AIConnectionState> OnStateChanged;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnectionError;
    public event Action<int> OnReconnectAttempt;

    private AIClient _aiClient;
    private int _reconnectAttempts;
    private Coroutine _reconnectCoroutine;

    public AIConnectionState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                var previousState = _currentState;
                _currentState = value;
                OnStateChanged?.Invoke(value);

                // Дополнительные специфичные события
                switch (value)
                {
                    case AIConnectionState.Connected:
                        OnConnected?.Invoke();
                        break;
                    case AIConnectionState.Disconnected:
                        OnDisconnected?.Invoke();
                        break;
                    case AIConnectionState.Error:
                        OnConnectionError?.Invoke();
                        break;
                }
            }
        }
    }

    void Awake()
    {
        _aiClient = FindObjectOfType<AIClient>();
        if (_aiClient == null)
        {
            Debug.LogError("AIClient not found in scene!");
            return;
        }

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        _aiClient.OnConnectionStatusChanged += HandleAIClientStatusChange;

        // Начальное состояние
        if (_aiClient.isConnected)
        {
            CurrentState = AIConnectionState.Connected;
        }
        else
        {
            CurrentState = AIConnectionState.Disconnected;
        }
    }

    private void HandleAIClientStatusChange(bool isConnected)
    {
        if (isConnected)
        {
            CurrentState = AIConnectionState.Connected;
            _reconnectAttempts = 0; // Сброс счетчика попыток
            StopReconnectCoroutine();
        }
        else
        {
            // Если было подключение и оно разорвалось
            if (CurrentState == AIConnectionState.Connected)
            {
                StartReconnection();
            }
            else
            {
                CurrentState = AIConnectionState.Disconnected;
            }
        }
    }

    public void StartConnection()
    {
        if (CurrentState == AIConnectionState.Connected)
            return;

        CurrentState = AIConnectionState.Connecting;
        _aiClient.RetryConnection();
    }

    public void StartReconnection()
    {
        if (_reconnectAttempts >= maxReconnectAttempts)
        {
            CurrentState = AIConnectionState.Error;
            return;
        }

        CurrentState = AIConnectionState.Reconnecting;
        _reconnectAttempts++;
        OnReconnectAttempt?.Invoke(_reconnectAttempts);

        _reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
    }

    private IEnumerator ReconnectCoroutine()
    {
        yield return new WaitForSeconds(reconnectInterval);

        Debug.Log($"Попытка переподключения {_reconnectAttempts}/{maxReconnectAttempts}");
        _aiClient.RetryConnection();
    }

    private void StopReconnectCoroutine()
    {
        if (_reconnectCoroutine != null)
        {
            StopCoroutine(_reconnectCoroutine);
            _reconnectCoroutine = null;
        }
    }

    public void ResetConnection()
    {
        StopReconnectCoroutine();
        _reconnectAttempts = 0;
        CurrentState = AIConnectionState.Disconnected;
    }

    void OnDestroy()
    {
        if (_aiClient != null)
        {
            _aiClient.OnConnectionStatusChanged -= HandleAIClientStatusChange;
        }
        StopReconnectCoroutine();
    }
}