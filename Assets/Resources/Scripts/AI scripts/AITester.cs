using UnityEngine;

public class AITester : MonoBehaviour
{
    [Header("References")]
    public AIConnectionStateMachine stateMachine;
    public AIClient aiClient;

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

        if (stateMachine != null)
        {
            // Подписываемся на все события State Machine
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
            ShowStatus("Нейросеть не подключена. Запустите LM Studio.", false);
            stateMachine.StartConnection();
        }
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

        // Автоматически отправляем тестовое сообщение при первом подключении
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

    private void SendTestMessage()
    {
        // Отправляем тестовое сообщение при успешном подключении
        aiClient.SendMessageToAI("Ответь одним словом: 'готов'");
    }

    void Update()
    {
        // Таймер для скрытия статуса
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (stateMachine.CurrentState == AIConnectionState.Connected)
            {
                aiClient.SendMessageToAI("Кто ты?");
            }
            else
            {
                ShowStatus("Невозможно отправить сообщение: нет подключения", true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            stateMachine.StartConnection();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            stateMachine.ResetConnection();
            ShowStatus("Подключение сброшено", false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Принудительное переподключение
            stateMachine.StartReconnection();
        }
    }

    void OnGUI()
    {
        if (_showStatus)
        {
            DrawStatusWindow();
        }

        DrawConnectionPanel();
    }

    private void DrawStatusWindow()
    {
        GUI.Box(new Rect(Screen.width - 310, 10, 300, 60), "Статус AI");
        GUI.Label(new Rect(Screen.width - 300, 30, 290, 40), _lastStatusMessage);
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
                    aiClient.SendMessageToAI("Ответь кратко: соединение работает?");
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
        // Здесь можно открыть панель с инструкцией
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