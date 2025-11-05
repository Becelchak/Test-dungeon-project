using EventBusSystem;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

public class ClassicalDialogueViewModel : BaseViewModel
{
    private readonly IDialogueService _dialogueService;
    private DialogueData _dialogueData;
    private DialogueNode _currentNode;

    private string _npcName;
    private string _dialogueText;
    private ObservableCollection<DialogueResponseVM> _responses = new();

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

    public ObservableCollection<DialogueResponseVM> Responses
    {
        get => _responses;
        private set => SetProperty(ref _responses, value);
    }

    public ICommand ResponseSelectedCommand { get; }

    public ClassicalDialogueViewModel(string dialogueId)
    {
        _dialogueService = ServiceLocator.Instance.GetService<IDialogueService>();
        ResponseSelectedCommand = new RelayCommand<string>(OnResponseSelected);

        EventBus.Subscribe(this as IDialogueEventSubscriber);

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

        foreach (var action in _currentNode.actions)
        {
            _dialogueService.ExecuteDialogueAction(action);
        }
    }

    private void UpdateResponses()
    {
        Responses.Clear();
        foreach (var response in _currentNode.responses)
        {
            if (CheckResponseConditions(response))
            {
                Responses.Add(new DialogueResponseVM
                {
                    ResponseId = response.responseId,
                    Text = response.text,
                    SelectCommand = ResponseSelectedCommand
                });
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
        EventBus.Unsubscribe(this as IDialogueEventSubscriber);
    }
}

// ViewModel для ответов
public class DialogueResponseVM
{
    public string ResponseId { get; set; }
    public string Text { get; set; }
    public ICommand SelectCommand { get; set; }
}