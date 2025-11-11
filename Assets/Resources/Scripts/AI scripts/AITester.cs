using UnityEngine;

public class AITester : MonoBehaviour
{
    [Header("References")]
    public AIConnectionStateMachine stateMachine;

    [Header("Test Settings")]
    public bool startAIDialogueOnConnect = true;
    public string testNpcId = "ai_skeleton";
    public string testDialogueId = "monk_knight";

    [Header("UI Settings")]
    public float statusDisplayTime = 3f;

    private IAIService _aiService;
    private IWindowService _windowService;
    private string _lastStatusMessage = "";
    private float _statusDisplayTimer = 0f;
    private bool _showStatus = false;
    private bool _isSendingMessage = false;

    private void Start()
    {
        InitializeServices();
        SubscribeToEvents();

        if (stateMachine.CurrentState == AIConnectionState.Connected && startAIDialogueOnConnect)
        {
            StartAIDialogueWithDelay();
        }
    }

    private void InitializeServices()
    {
        if (stateMachine == null)
            stateMachine = FindObjectOfType<AIConnectionStateMachine>();

        _aiService = ServiceLocator.Instance.GetService<IAIService>();
        _windowService = ServiceLocator.Instance.GetService<IWindowService>();

        if (_aiService == null) Debug.LogError("[AITester] IAIService не найден");
        if (_windowService == null) Debug.LogError("[AITester] IWindowService не найден");
    }

    private void SubscribeToEvents()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged += HandleStateChanged;
            stateMachine.OnConnected += HandleConnected;
            stateMachine.OnDisconnected += HandleDisconnected;
            stateMachine.OnConnectionError += HandleConnectionError;
            stateMachine.OnReconnectAttempt += HandleReconnectAttempt;
        }
    }

    private void HandleStateChanged(AIConnectionState newState)
    {
        switch (newState)
        {
            case AIConnectionState.Connecting:
                ShowStatus("Подключение к нейросети...");
                break;
            case AIConnectionState.Reconnecting:
                ShowStatus("Переподключение...");
                break;
            case AIConnectionState.Error:
                ShowStatus("Ошибка подключения. Проверьте LM Studio.", true);
                break;
        }
    }

    private void HandleConnected()
    {
        ShowStatus("Нейросеть подключена!");

        if (startAIDialogueOnConnect)
        {
            StartAIDialogueWithDelay();
        }
    }

    private void HandleDisconnected() => ShowStatus("Соединение разорвано", true);
    private void HandleConnectionError() => ShowStatus("Ошибка соединения", true);

    private void HandleReconnectAttempt(int attempt) =>
        ShowStatus($"Переподключение ({attempt}/{stateMachine.maxReconnectAttempts})");

    private void ShowStatus(string message, bool isWarning = false)
    {
        _lastStatusMessage = message;
        _showStatus = true;
        _statusDisplayTimer = statusDisplayTime;
    }

    private void StartAIDialogueWithDelay()
    {
        Invoke(nameof(StartAIDialogue), 1f);
    }

    private void StartAIDialogue()
    {
        if (_windowService != null && stateMachine.CurrentState == AIConnectionState.Connected)
        {
            _windowService.ShowAIDialogue(testNpcId);
        }
        else
        {
            ShowStatus("Невозможно начать диалог: нет подключения", true);
        }
    }

    private void StartClassicalDialogue()
    {
        if (_windowService != null)
        {
            _windowService.ShowClassicalDialogue(testDialogueId);
        }
    }

    private void SendTestMessage()
    {
        if (_isSendingMessage) return;

        if (stateMachine.CurrentState == AIConnectionState.Connected)
        {
            _isSendingMessage = true;
            _aiService?.SendMessage("Ответь кратко: соединение работает");

            // Сброс флага через время таймаута для защиты от зависания
            Invoke(nameof(ResetSendingFlag), 10f);
        }
        else
        {
            ShowStatus("Невозможно отправить сообщение: нет подключения", true);
        }
    }

    private void ResetSendingFlag()
    {
        _isSendingMessage = false;
    }

    private void Update()
    {
        UpdateStatusTimer();
        HandleTestInput();
    }

    private void UpdateStatusTimer()
    {
        if (_showStatus)
        {
            _statusDisplayTimer -= Time.deltaTime;
            if (_statusDisplayTimer <= 0) _showStatus = false;
        }
    }

    private void HandleTestInput()
    {
        if (Input.GetKeyDown(KeyCode.F1)) StartAIDialogue();
        if (Input.GetKeyDown(KeyCode.F2)) StartClassicalDialogue();
        if (Input.GetKeyDown(KeyCode.F3)) ShowInstructions();
        if (Input.GetKeyDown(KeyCode.F4)) _aiService?.ClearConversation();
    }

    private void OnGUI()
    {
        DrawConnectionPanel();
        if (_showStatus) DrawStatusWindow();
    }

    private void DrawConnectionPanel()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));

        GUILayout.Box($"Состояние AI: {stateMachine.CurrentState}");

        switch (stateMachine.CurrentState)
        {
            case AIConnectionState.Disconnected:
                if (GUILayout.Button("Подключиться", GUILayout.Height(30)))
                    stateMachine.StartConnection();
                break;

            case AIConnectionState.Connected:
                GUILayout.Label("Нейросеть активна");
                if (GUILayout.Button("Тестовый запрос", GUILayout.Height(25)) && !_isSendingMessage)
                    SendTestMessage();
                break;

            case AIConnectionState.Error:
                GUILayout.Label("Превышены попытки подключения");
                if (GUILayout.Button("Сбросить и повторить", GUILayout.Height(30)))
                {
                    stateMachine.ResetConnection();
                    stateMachine.StartConnection();
                }
                break;

            case AIConnectionState.Reconnecting:
                GUILayout.Label("Идет переподключение...");
                if (GUILayout.Button("Отменить", GUILayout.Height(25)))
                    stateMachine.ResetConnection();
                break;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("AI Диалог (F1)", GUILayout.Height(25)))
            StartAIDialogue();

        if (GUILayout.Button("Классический диалог (F2)", GUILayout.Height(25)))
            StartClassicalDialogue();

        if (GUILayout.Button("Очистить историю (F4)", GUILayout.Height(25)))
            _aiService?.ClearConversation();

        if (GUILayout.Button("Инструкция (F3)", GUILayout.Height(25)))
            ShowInstructions();

        GUILayout.EndArea();
    }

    private void DrawStatusWindow()
    {
        GUI.Box(new Rect(Screen.width - 310, 10, 300, 50), "Статус AI");
        GUI.Label(new Rect(Screen.width - 300, 30, 290, 30), _lastStatusMessage);
    }

    private void ShowInstructions()
    {
        string instructionsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            "Инструкция по установке AI.txt"
        );

        if (System.IO.File.Exists(instructionsPath))
        {
            Application.OpenURL(instructionsPath);
        }
        else
        {
            Debug.LogWarning("[AITester] Файл инструкции не найден: " + instructionsPath);
        }
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= HandleStateChanged;
            stateMachine.OnConnected -= HandleConnected;
            stateMachine.OnDisconnected -= HandleDisconnected;
            stateMachine.OnConnectionError -= HandleConnectionError;
            stateMachine.OnReconnectAttempt -= HandleReconnectAttempt;
        }
    }
}