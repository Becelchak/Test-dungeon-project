using UnityEngine;

public class DialogueService : MonoBehaviour, IDialogueService
{
    public bool CheckCondition(DialogueCondition condition)
    {
        throw new System.NotImplementedException();
    }

    public void ExecuteDialogueAction(DialogueAction action)
    {
        throw new System.NotImplementedException();
    }

    public AIDialogueData GetAIDialogue(string npcId)
    {
        throw new System.NotImplementedException();
    }

    public DialogueData GetDialogue(string dialogueId)
    {
        throw new System.NotImplementedException();
    }
}
