using System;
using UnityEngine;

public class DialogueService : BaseService, IDialogueService
{
    protected override Type GetServiceType() => typeof(IDialogueService);

    public DialogueData GetDialogue(string dialogueId)
    {
        try
        {
            var resourceService = ServiceLocator.Instance.GetService<IResourceService>();
            if (resourceService == null)
            {
                Debug.LogError("ResourceService not available");
                return null;
            }

            return resourceService.LoadJson<DialogueData>($"Dialogues/{dialogueId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load dialogue {dialogueId}: {e.Message}");
            return null;
        }
    }

    public AIDialogueData GetAIDialogue(string npcId)
    {
        try
        {
            var resourceService = ServiceLocator.Instance.GetService<IResourceService>();
            if (resourceService == null)
            {
                Debug.LogError("ResourceService not available");
                return null;
            }

            return resourceService.LoadJson<AIDialogueData>($"AIDialogues/{npcId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load AI dialogue for {npcId}: {e.Message}");
            return null;
        }
    }

    public void ExecuteDialogueAction(DialogueAction action)
    {
        Debug.Log($"Executing dialogue action: {action.type} - {action.actionId}");
        // Здесь будет логика выполнения действий (выдача предметов, старт квестов и т.д.)
    }

    public bool CheckCondition(DialogueCondition condition)
    {
        // Временная реализация - всегда возвращает true
        // В будущем здесь будет проверка условий (прогресс квестов, наличие предметов и т.д.)
        return true;
    }
}
