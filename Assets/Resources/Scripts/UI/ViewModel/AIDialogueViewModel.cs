using EventBusSystem;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityEngine;

public class AIDialogueViewModel : BaseViewModel
{
    private IAIService _aiService;
    private AIDialogueData _aiData;
    private PlayerProfileService _player;
    private IPlayerContextService _playerContextService;

    private string _npcName = "Загрузка...";
    private string _dialogueText = "...";
    private string _userInput;
    private bool _isWaitingForResponse;
    private bool _isInitialized = false;

    public string NpcName
    {
        get => _npcName;
        private set => SetProperty(ref _npcName, value);
    }

    public string DialogueText
    {
        get => _dialogueText;
        private set => SetProperty(ref _dialogueText, value);
    }

    public string UserInput
    {
        get => _userInput;
        set => SetProperty(ref _userInput, value);
    }

    public bool IsWaitingForResponse
    {
        get => _isWaitingForResponse;
        private set => SetProperty(ref _isWaitingForResponse, value);
    }

    public bool IsInitialized => _isInitialized;

    public ICommand SendMessageCommand { get; }
    public ICommand CloseDialogueCommand { get; }
    public DialogueLogViewModel LogViewModel { get; }
    public AIDialogueViewModel(string npcId, DialogueLogViewModel logViewModel)
    {
        LogViewModel = logViewModel;
        SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        CloseDialogueCommand = new RelayCommand(CloseDialogue);

        // Начинаем асинхронную инициализацию
        InitializeAsync(npcId);
    }

    private async void InitializeAsync(string npcId)
    {
        try
        {
            var dialogueService = ServiceLocator.Instance.GetService<IDialogueService>();
            _aiService = ServiceLocator.Instance.GetService<IAIService>();
            _player = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
            _playerContextService = ServiceLocator.Instance.GetService<IPlayerContextService>();
            if (_playerContextService != null)
                _playerContextService.Initialize();

            if (dialogueService == null || _aiService == null || _player == null)
            {
                Debug.LogError($"Не удалось получить необходимые сервисы. Dialogue = {dialogueService}, AI = {_aiService}, Player = {_player}");
                return;
            }

            _aiData = dialogueService.GetAIDialogue(npcId);

            if (_aiData != null)
            {
                _aiData.currentEmotions = _aiData.defaultEmotions?.ToArray() ?? new float[8];
                EventBus.RaiseEvent<IEmotionsUpdatedSubscriber>(
                    s => s.OnEmotionsUpdated(new EmotionsUpdatedEvent(_aiData.currentEmotions))
                );
            }

            if (_aiData == null)
            {
                Debug.LogError($"Не удалось загрузить данные AI диалога для NPC: {npcId}");
                NpcName = "Ошибка загрузки";
                return;
            }

            // Теперь инициализируем данные
            InitializeAIDialogue();
            _isInitialized = true;

            // Уведомляем об изменении состояния инициализации
            OnPropertyChanged(nameof(IsInitialized));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка инициализации AI диалога: {e.Message}");
            NpcName = "Ошибка";
        }
    }

    private void InitializeAIDialogue()
    {
        NpcName = _aiData.npcName;
        DialogueText = "*Безмолвно ждет вашего вопроса*";

        LogViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, DialogueText);

        if (_aiData.npcPortrait != null)
        {
            Debug.Log($"Портрет загружен: {_aiData.npcPortrait.name}");
        }

        _aiService.OnAIResponseReceived += OnAIResponse;
        _aiService.OnConnectionStatusChanged += OnConnectionStatusChanged;

        EventBus.RaiseEvent<IDialogueEventSubscriber>(s => s.OnDialogueStarted(_aiData.npcId, DialogueType.AI));
    }

    private bool CanSendMessage()
    {
        return !string.IsNullOrWhiteSpace(UserInput) && !IsWaitingForResponse;
    }

    private void SendMessage()
    {
        if (!CanSendMessage()) return;

        var userMessage = UserInput;
        UserInput = string.Empty;
        IsWaitingForResponse = true;

        var fullPrompt = BuildFullPrompt(userMessage);
        _aiService.SendMessage(fullPrompt);

        LogViewModel.AddEntry(_player.CurrentProfile.playerName, _player.CurrentProfile.avatar, userMessage, true);
        
    }

    /// <summary>
    /// Составляет полный промпт для LLM в ходе диалога. Помимо самого сообщения от игрока, передает его текущие показатели и контекст.
    /// </summary>
    /// <param name="userMessage">Сообщение пользователя</param>
    /// <returns></returns>
    private string BuildFullPrompt(string userMessage)
    {
        // Получаем контекст игрока
        string playerContext = _playerContextService.GetPlayerContextForAI();
        // Для отладки
        Debug.Log("=== PLAYER CONTEXT SENT TO AI ===");
        Debug.Log(playerContext);
        Debug.Log("=================================");

        // Формируем строку с текущими эмоциями для контекста
        string emotionsStr = string.Join(", ", _aiData.currentEmotions.Select(e => e.ToString("F2")));

        return $@"

        ДАННЫЕ ИГРОКА:
        {playerContext}

        ТВОИ ТЕКУЩИЕ ЭМОЦИИ (joy, sadness, anger, fear, surprise, trust, arousal, dominance):
        [{emotionsStr}]

        ОГРАНИЧЕНИЯ:
        {string.Join("\n", _aiData.constraints.Select(c => $"- {c.constraint}: {c.value}"))}

        ТЕКУЩИЙ ДИАЛОГ:
        Пользователь: {userMessage}
        {_aiData.npcName}:

        ОТВЕЧАЙ ТОЛЬКО В ФОРМАТЕ JSON (без дополнительного текста):
        {{
          ""reply"": ""текст ответа NPC"",
          ""emotions"": [joy, sadness, anger, fear, surprise, trust, arousal, dominance]
        }}
        Где каждое значение эмоции от 0 до 1. Эмоции должны меняться в зависимости от хода диалога.";
    }

    private void OnAIResponse(string response)
    {
        IsWaitingForResponse = false;

        // Пытаемся извлечь JSON из ответа
        string json = ExtractJson(response);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var obj = JObject.Parse(json);
                string reply = obj["reply"]?.ToString();
                var emotionsToken = obj["emotions"];

                if (!string.IsNullOrEmpty(reply) && emotionsToken != null && emotionsToken.Type == JTokenType.Array)
                {
                    float[] newEmotions = new float[8];
                    for (int i = 0; i < 8; i++)
                        newEmotions[i] = (float)emotionsToken[i];

                    // Обновляем текущие эмоции NPC
                    _aiData.currentEmotions = newEmotions;

                    // Сохраняем в профиль игрока
                    _player.CurrentProfile.LastEmotions = newEmotions;
                    _player.SaveProfile(_player.CurrentProfile);

                    // Отправляем событие для ML-агента
                    EventBus.RaiseEvent<IEmotionsUpdatedSubscriber>(
                        s => s.OnEmotionsUpdated(new EmotionsUpdatedEvent(newEmotions))
                    );

                    LogViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, reply);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Ошибка парсинга JSON: {e.Message}. Ответ: {response}");
            }
        }

        LogViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, response);
    }

    private string ExtractJson(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        int start = input.IndexOf('{');
        int end = input.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return input.Substring(start, end - start + 1);
        }
        return null;
    }

    private void OnConnectionStatusChanged(bool isConnected)
    {
        if (!isConnected)
        {
            DialogueText = "*Собеседник потерял дар речи*";
            LogViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, DialogueText);
        }
    }

    private void CloseDialogue()
    {
        EventBus.RaiseEvent<IDialogueEventSubscriber>(s => s.OnDialogueEnded());
        Cleanup();
    }

    public override void Initialize() { }

    public override void Cleanup()
    {
        _aiService.OnAIResponseReceived -= OnAIResponse;
        _aiService.OnConnectionStatusChanged -= OnConnectionStatusChanged;

        _aiService.BreakeMessage();
        LogViewModel.ClearLog();

        var windowService = ServiceLocator.Instance.GetService<IWindowService>();
        windowService?.CloseWindow<AIDialogueViewModel>();
        //EventBus.Unsubscribe(this as IDialogueEventSubscriber);
    }
}