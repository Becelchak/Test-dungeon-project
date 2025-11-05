using UnityEngine;

public class AITester : MonoBehaviour
{
    [Header("References")]
    public AIConnectionStateMachine stateMachine;
    public AIClient aiClient;

    [Header("Test Settings")]
    public bool startAIDialogueOnConnect = true;
    public string testNpcId = "ai_skeleton";
    public string testDialogueId = "monk_knight";
    private IAIService _aiService;
    private IWindowService _windowService;

    [Header("UI Settings")]
    public float statusDisplayTime = 3f;

    private string _lastStatusMessage = "";
    private float _statusDisplayTimer = 0f;
    private bool _showStatus = false;

    void Start()
    {
        if (stateMachine == null)
            stateMachine = FindObjectOfType<AIConnectionStateMachine>();
        if (aiClient == null)
            aiClient = FindObjectOfType<AIClient>();
        if(_aiService == null)
            _aiService = ServiceLocator.Instance.GetService<IAIService>();
        if(_windowService == null)
            _windowService = ServiceLocator.Instance.GetService<IWindowService>();

        if (stateMachine != null)
        {
            stateMachine.OnStateChanged += HandleStateChanged;
            stateMachine.OnConnected += HandleConnected;
            stateMachine.OnDisconnected += HandleDisconnected;
            stateMachine.OnConnectionError += HandleConnectionError;
            stateMachine.OnReconnectAttempt += HandleReconnectAttempt;
        }

        // Начальная проверка подключения
        CheckInitialConnection();
    }

    private void CheckInitialConnection()
    {
        if (!aiClient.isConnected)
        {
            stateMachine.StartConnection();
        }
        else
            Debug.Log("AITester started. Press F1 for AI dialogue, F2 for classical dialogue.");
    }

    private void HandleStateChanged(AIConnectionState newState)
    {
        Debug.Log($"Состояние подключения изменилось: {newState}");

        switch (newState)
        {
            case AIConnectionState.Connecting:
                ShowStatus("Подключение к нейросети...", false);
                break;
            case AIConnectionState.Reconnecting:
                ShowStatus("Переподключение...", false);
                break;
            case AIConnectionState.Error:
                ShowStatus("Ошибка подключения. Проверьте LM Studio.", true);
                break;
        }
    }

    private void HandleConnected()
    {
        ShowStatus("Нейросеть подключена!", false);

        SendTestMessage();
    }

    private void HandleDisconnected()
    {
        ShowStatus("Соединение с нейросетью разорвано", true);
    }

    private void HandleConnectionError()
    {
        ShowStatus("Ошибка соединения с нейросетью", true);
    }

    private void HandleReconnectAttempt(int attempt)
    {
        ShowStatus($"Попытка переподключения ({attempt}/{stateMachine.maxReconnectAttempts})", false);
    }

    private void ShowStatus(string message, bool isError = false)
    {
        _lastStatusMessage = message;
        _showStatus = true;
        _statusDisplayTimer = statusDisplayTime;

        if (isError)
        {
            Debug.LogWarning($"AI Status: {message}");
        }
        else
        {
            Debug.Log($"AI Status: {message}");
        }
    }

    private void StartAIDialogue()
    {
        if (_windowService != null)
        {
            _windowService.ShowAIDialogue(testNpcId);
            Debug.Log($"Started AI dialogue with {testNpcId}");
        }
        else
        {
            Debug.LogError("WindowService not available!");
        }
    }

    private void StartClassicalDialogue()
    {
        if (_windowService != null)
        {
            _windowService.ShowClassicalDialogue(testDialogueId);
            Debug.Log($"Started classical dialogue: {testDialogueId}");
        }
        else
        {
            Debug.LogError("WindowService not available!");
        }
    }
    private void SendTestMessage()
    {
        // Отправляет сообщение при успешном подключении
        aiClient.SendMessageToAI("Ответь одним словом: 'готов'");
    }

    void Update()
    {
        if (_showStatus)
        {
            _statusDisplayTimer -= Time.deltaTime;
            if (_statusDisplayTimer <= 0)
            {
                _showStatus = false;
            }
        }
        // Обработка клавиш для тестирования
        HandleTestInput();
    }

    private void HandleTestInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartAIDialogue();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            StartClassicalDialogue();
        }
    }

    void OnGUI()
    {
        DrawConnectionPanel();
    }

    private void DrawConnectionPanel()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Box($"Состояние: {stateMachine.CurrentState}");

        switch (stateMachine.CurrentState)
        {
            case AIConnectionState.Disconnected:
                if (GUILayout.Button("Подключиться", GUILayout.Height(30)))
                {
                    stateMachine.StartConnection();
                }
                break;

            case AIConnectionState.Connected:
                GUILayout.Label("Нейросеть активна");
                if (GUILayout.Button("Тестовый запрос", GUILayout.Height(30)))
                {
                    aiClient.SendMessageToAI("Ответь кратко: соединение работает");
                }
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
                if (GUILayout.Button("Отменить", GUILayout.Height(30)))
                {
                    stateMachine.ResetConnection();
                }
                break;
        }

        if (GUILayout.Button("Показать инструкцию", GUILayout.Height(25)))
        {
            ShowInstructions();
        }
        GUILayout.EndArea();
    }

    private void ShowInstructions()
    {
        Debug.Log("Открытие инструкции по подключению нейросети...");
        Application.OpenURL(Application.streamingAssetsPath + "/Инструкция по установке AI.txt");
    }

    void OnDestroy()
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