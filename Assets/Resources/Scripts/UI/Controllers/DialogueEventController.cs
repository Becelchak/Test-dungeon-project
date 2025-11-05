using EventBusSystem;
using UnityEngine;

public class DialogueEventController : MonoBehaviour, IDialogueEventSubscriber
{
    private void OnEnable()
    {
        EventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    public void OnDialogueStarted(string npcId, DialogueType dialogueType)
    {
        Debug.Log($"Диалог начался с NPC: {npcId}, тип: {dialogueType}");

        Time.timeScale = 0;
    }

    public void OnDialogueEnded()
    {
        Debug.Log("Диалог завершен");

        Time.timeScale = 1;
    }

    public void OnResponseSelected(string responseId)
    {
        Debug.Log($"Выбран ответ: {responseId}");
    }
}