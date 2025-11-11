using EventBusSystem;
using System.Linq;
using System.Windows.Input;

public class AIDialogueViewModel : BaseViewModel
{
    private readonly IAIService _aiService;
    private readonly AIDialogueData _aiData;
    private readonly PlayerProfileService _player;

    private string _npcName;
    private string _dialogueText;
    private string _userInput;
    private bool _isWaitingForResponse;

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

    public ICommand SendMessageCommand { get; }
    public ICommand CloseDialogueCommand { get; }
    private DialogueLogViewModel _logViewModel;

    public AIDialogueViewModel(string npcId, DialogueLogViewModel logViewModel)
    {
        _logViewModel = logViewModel;
        var dialogueService = ServiceLocator.Instance.GetService<IDialogueService>();
        _aiService = ServiceLocator.Instance.GetService<IAIService>();
        _aiData = dialogueService.GetAIDialogue(npcId);
        _player = ServiceLocator.Instance.GetService<PlayerProfileService>();

        SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        CloseDialogueCommand = new RelayCommand(CloseDialogue);

        EventBus.Subscribe(this as IDialogueEventSubscriber);

        InitializeAIDialogue();
    }

    private void InitializeAIDialogue()
    {
        NpcName = _aiData.npcName;
        DialogueText = "...";

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

        _logViewModel.AddEntry("Игрок", _player.CurrentProfile.avatar, UserInput, true);
    }

    private string BuildFullPrompt(string userMessage)
    {
        return $@"{_aiData.initialPrompt}

            Ограничения:
            {string.Join("\n", _aiData.constraints.Select(c => $"- {c.constraint}: {c.value}"))}

            Текущий диалог:
            Пользователь: {userMessage}
            {_aiData.npcName}:";
    }

    private void OnAIResponse(string response)
    {
        IsWaitingForResponse = false;
        DialogueText = response;
        _logViewModel.AddEntry(_aiData.npcName, _aiData.npcPortrait, response);
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
        EventBus.Unsubscribe(this as IDialogueEventSubscriber);
    }
}