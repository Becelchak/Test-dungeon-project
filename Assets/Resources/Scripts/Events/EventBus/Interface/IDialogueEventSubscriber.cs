using EventBusSystem;

public interface IDialogueEventSubscriber : IGlobalSubscriber
{
    void OnDialogueStarted(string npcId, DialogueType dialogueType);
    void OnDialogueEnded();
    void OnResponseSelected(string responseId);
}

public enum DialogueType
{
    Classical,
    AI
}