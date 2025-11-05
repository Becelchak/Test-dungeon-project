public interface IDialogueService
{
    DialogueData GetDialogue(string dialogueId);
    AIDialogueData GetAIDialogue(string npcId);
    void ExecuteDialogueAction(DialogueAction action);
    bool CheckCondition(DialogueCondition condition);
}