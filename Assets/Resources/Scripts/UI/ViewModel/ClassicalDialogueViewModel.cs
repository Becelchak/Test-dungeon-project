using Cysharp.Threading.Tasks;
using EventBusSystem;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

public class ClassicalDialogueViewModel : BaseViewModel
{
    private IDialogueService _dialogueService;
    private PlayerProfileService _player;
    private DialogueData _dialogueData;
    private DialogueNode _currentNode;

    private string _npcName;
    private string _dialogueText;
    private string _dialogueId;
    private ObservableCollection<DialogueResponseViewModel> _responses = new();
    public DialogueLogViewModel LogViewModel { get; private set; }

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

    public ICommand ResponseSelectedCommand { get; private set; }

    public ClassicalDialogueViewModel()
    {

    }

    public void Setup(string npcId)
    {
        _dialogueId = npcId;
        LogViewModel = new DialogueLogViewModel(); // Создаем зависимый логгер
        ResponseSelectedCommand = new RelayCommand<string>(OnResponseSelected);
    }

    public override void Initialize()
    {
        _dialogueService = ServiceLocator.Instance.GetService<IDialogueService>();
        _player = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();

        if (string.IsNullOrEmpty(_dialogueId))
        {
            UnityEngine.Debug.LogError("[ClassicalDialogue] Инициализация вызвана без предварительного вызова Setup!");
            return;
        }

        LoadDialogue(_dialogueId);
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

    private async void OnResponseSelected(string responseId)
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
            await UniTask.WaitForSeconds(0.5f);
            SetCurrentNode(response.nextNodeId);
        }
    }

    private void EndDialogue()
    {
        EventBus.RaiseEvent<IDialogueEventSubscriber>(s => s.OnDialogueEnded());
        Cleanup();
        ServiceLocator.Instance.GetService<IWindowService>()?.CloseWindow<ClassicalDialogueViewModel>();
    }
    public override void Cleanup() 
    {

    }
}