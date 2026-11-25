public interface IDialogueService
{
    DialogueData GetDialogue(string dialogueId);
    AIDialogueData GetAIDialogue(string npcId);
    void ExecuteDialogueAction(DialogueAction action);
    bool CheckCondition(DialogueCondition condition);

    /// <summary>Получить запись памяти об NPC (null — ещё не общались).</summary>
    NpcMemoryEntry GetMemory(string npcId);
    /// <summary>Сохранить обмен репликами с NPC в память.</summary>
    void SaveExchange(string npcId, string playerMessage, string npcReply, float[] emotions);
}