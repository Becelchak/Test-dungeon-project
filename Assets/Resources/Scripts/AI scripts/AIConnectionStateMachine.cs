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

    public event Action<AIConnectionState> OnStateChanged;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnectionError;
    public event Action<int> OnReconnectAttempt;

    private IAIService _aiService;
    private int _reconnectAttempts;
    private Coroutine _reconnectCoroutine;

    public AIConnectionState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                OnStateChanged?.Invoke(value);

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

    private void Awake()
    {
        _aiService = ServiceLocator.Instance.GetService<IAIService>();
        if (_aiService == null)
        {
            Debug.LogError("[StateMachine] IAIService не найден!");
            return;
        }

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        _aiService.OnConnectionStatusChanged += HandleAIClientStatusChange;
        _aiService.OnConnectionError += HandleAIClientError;

        // Начальное состояние
        CurrentState = _aiService.IsConnected ?
            AIConnectionState.Connected :
            AIConnectionState.Disconnected;
    }

    private void HandleAIClientStatusChange(bool isConnected)
    {
        if (isConnected)
        {
            CurrentState = AIConnectionState.Connected;
            _reconnectAttempts = 0; // Сброс счетчика при успешном подключении
            StopReconnectCoroutine();
        }
        else
        {
            if (CurrentState == AIConnectionState.Connected)
            {
                // Было подключение, но соединение разорвано
                StartReconnection();
            }
            else
            {
                CurrentState = AIConnectionState.Disconnected;
            }
        }
    }

    private void HandleAIClientError(string error)
    {
        Debug.LogWarning($"[StateMachine] Ошибка подключения: {error}");

        // При любой ошибке пытаемся переподключиться, если были подключены
        if (CurrentState == AIConnectionState.Connected)
        {
            StartReconnection();
        }
    }

    public void StartConnection()
    {
        if (CurrentState == AIConnectionState.Connected ||
            CurrentState == AIConnectionState.Connecting)
            return;

        CurrentState = AIConnectionState.Connecting;
        _aiService.RetryConnection();
    }

    public void StartReconnection()
    {
        if (_reconnectAttempts >= maxReconnectAttempts)
        {
            CurrentState = AIConnectionState.Error;
            Debug.LogError("[StateMachine] Превышено максимальное количество попыток переподключения");
            return;
        }

        CurrentState = AIConnectionState.Reconnecting;
        _reconnectAttempts++;

        Debug.Log($"[StateMachine] Попытка переподключения {_reconnectAttempts}/{maxReconnectAttempts}");
        OnReconnectAttempt?.Invoke(_reconnectAttempts);

        _reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
    }

    private IEnumerator ReconnectCoroutine()
    {
        yield return new WaitForSeconds(reconnectInterval);
        _aiService.RetryConnection();
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
        Debug.Log("[StateMachine] Сброс состояния подключения");
    }

    private void OnDestroy()
    {
        if (_aiService != null)
        {
            _aiService.OnConnectionStatusChanged -= HandleAIClientStatusChange;
            _aiService.OnConnectionError -= HandleAIClientError;
        }
        StopReconnectCoroutine();
    }
}