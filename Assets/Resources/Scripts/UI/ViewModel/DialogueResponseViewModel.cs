using System.Windows.Input;

public class DialogueResponseViewModel : BaseViewModel
{
    public string ResponseId { get; set; }
    public string Text { get; set; }
    public string NextNodeId { get; set; }
    public ICommand SelectCommand { get; set; }

    public DialogueResponseViewModel(DialogueResponse response, ICommand selectCommand)
    {
        ResponseId = response.responseId;
        Text = response.text;
        NextNodeId = response.nextNodeId;
        SelectCommand = selectCommand;
    }

    public override void Cleanup()
    {
    }

    public override void Initialize()
    {
    }
}