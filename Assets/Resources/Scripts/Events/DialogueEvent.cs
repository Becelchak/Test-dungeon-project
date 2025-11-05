using EventBusSystem;

public struct DialogueEvent : IDialogueEventSubscriber
{
    public string NpcId { get; }
    public DialogueType Type { get; }

    public void OnDialogueEnded()
    {

    }

    public void OnDialogueStarted(string npcId, DialogueType dialogueType)
    {

    }

    public void OnResponseSelected(string responseId)
    {

    }
}