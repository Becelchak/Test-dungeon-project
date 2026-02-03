using EventBusSystem;
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
                Debug.LogError("Не удалось получить необходимые сервисы");
                return;
            }

            _aiData = dialogueService.GetAIDialogue(npcId);

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

    private string BuildFullPrompt(string userMessage)
    {
        // Получаем контекст игрока
        string playerContext = _playerContextService.GetPlayerContextForAI();
        // Для отладки
        Debug.Log("=== PLAYER CONTEXT SENT TO AI ===");
        Debug.Log(playerContext);
        Debug.Log("=================================");

        return $@"{_aiData.initialPrompt}

                ДАННЫЕ ИГРОКА:
                {playerContext}

                ОГРАНИЧЕНИЯ:
                {string.Join("\n", _aiData.constraints.Select(c => $"- {c.constraint}: {c.value}"))}

                ТЕКУЩИЙ ДИАЛОГ:
                Пользователь: {userMessage}
                {_aiData.npcName}:";
    }

    private void OnAIResponse(string response)
    {
        IsWaitingForResponse = false;
        LogViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, response);
    }

    private void OnConnectionStatusChanged(bool isConnected)
    {
        if (!isConnected)
        {
            DialogueText = "Извините, нейросеть временно недоступна.";
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