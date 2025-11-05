[System.Serializable]
public class DialogueData
{
    public string dialogueId;
    public string npcName;
    public string npcId;
    public DialogueNode[] nodes;
    public string startNodeId;
}

[System.Serializable]
public class DialogueNode
{
    public string nodeId;
    public string text;
    public DialogueCondition[] conditions;
    public DialogueAction[] actions;
    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseId;
    public string text;
    public string nextNodeId;
    public DialogueCondition[] conditions;
    public DialogueAction[] onSelected;
}

[System.Serializable]
public class DialogueCondition
{
    public string type; // "quest", "item", "flag"
    public string conditionId;
    public string value;
    public bool expectedResult = true;
}

[System.Serializable]
public class DialogueAction
{
    public string type; // "start_quest", "complete_quest", "give_item", "set_flag"
    public string actionId;
    public string value;
}

[System.Serializable]
public class AIDialogueData
{
    public string npcId;
    public string npcName;
    public string initialPrompt;
    public AIDialogueConstraint[] constraints;
    public string personalityProfile;
}

[System.Serializable]
public class AIDialogueConstraint
{
    public string type; // "topic_restriction", "response_length", "tone"
    public string constraint;
    public string value;
}