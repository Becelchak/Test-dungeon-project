using EventBusSystem;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

public class ClassicalDialogueViewModel : BaseViewModel
{
    private readonly IDialogueService _dialogueService;
    private readonly PlayerProfileService _player;
    private DialogueData _dialogueData;
    private DialogueNode _currentNode;

    private string _npcName;
    private string _dialogueText;
    private ObservableCollection<DialogueResponseViewModel> _responses = new();
    public DialogueLogViewModel LogViewModel { get; }

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

    public ObservableCollection<DialogueResponseViewModel> Responses
    {
        get => _responses;
        private set => SetProperty(ref _responses, value);
    }

    public ICommand ResponseSelectedCommand { get; }

    public ClassicalDialogueViewModel(string dialogueId, DialogueLogViewModel logViewModel)
    {
        LogViewModel = logViewModel;
        _dialogueService = ServiceLocator.Instance.GetService<IDialogueService>();
        _player = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
        ResponseSelectedCommand = new RelayCommand<string>(OnResponseSelected);

        //EventBus.Subscribe(this as IDialogueEventSubscriber);

        LoadDialogue(dialogueId);
    }

    private void LoadDialogue(string dialogueId)
    {
        _dialogueData = _dialogueService.GetDialogue(dialogueId);
        NpcName = _dialogueData.npcName;
        StartDialogue();
    }

    private void StartDialogue()
    {
        SetCurrentNode(_dialogueData.startNodeId);
        EventBus.RaiseEvent<IDialogueEventSubscriber>(s =>
            s.OnDialogueStarted(_dialogueData.npcId, DialogueType.Classical));
    }

    private void SetCurrentNode(string nodeId)
    {
        _currentNode = _dialogueData.nodes.FirstOrDefault(n => n.nodeId == nodeId);
        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }

        DialogueText = _currentNode.text;
        UpdateResponses();

        if (_currentNode.actions != null) 
        {
            foreach (var action in _currentNode.actions)
            {
                _dialogueService.ExecuteDialogueAction(action);
            }
        }
        LogViewModel.AddEntry(_dialogueData.npcName, _dialogueData.npcPortrait, _currentNode.text);
    }

    private void UpdateResponses()
    {
        Responses.Clear();
        foreach (var response in _currentNode.responses)
        {
            if (CheckResponseConditions(response))
            {
                Responses.Add(new DialogueResponseViewModel(response, ResponseSelectedCommand));
            }
        }
    }

    private bool CheckResponseConditions(DialogueResponse response)
    {
        foreach (var condition in response.conditions)
        {
            if (!_dialogueService.CheckCondition(condition))
                return false;
        }
        return true;
    }

    private void OnResponseSelected(string responseId)
    {
        var response = _currentNode.responses.FirstOrDefault(r => r.responseId == responseId);
        if (response != null)
        {
            LogViewModel.AddEntry(_player.CurrentProfile.playerName, _player.CurrentProfile.avatar, response.text, true);
            foreach (var action in response.onSelected)
            {
                _dialogueService.ExecuteDialogueAction(action);
            }

            EventBus.RaiseEvent<IDialogueEventSubscriber>( s => s.OnResponseSelected(responseId));
            SetCurrentNode(response.nextNodeId);
        }
    }

    private void EndDialogue()
    {
        EventBus.RaiseEvent<IDialogueEventSubscriber>(s => s.OnDialogueEnded());
        Cleanup();
    }

    public override void Initialize() { }
    public override void Cleanup() 
    {
        //EventBus.Unsubscribe(this as IDialogueEventSubscriber);
        var windowService = ServiceLocator.Instance.GetService<IWindowService>();
        windowService?.CloseWindow<ClassicalDialogueViewModel>();
    }
}