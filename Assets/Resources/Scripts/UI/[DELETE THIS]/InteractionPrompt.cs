using EventBusSystem;
using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour, IInteractionPromptEventSubscriber
{
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        promptPanel.SetActive(false);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    public void OnInteractionPrompt(InteractionPromptEvent evt)
    {
        Debug.Log($"Получено событие: Show={evt.Show}, текст='{evt.PromptText}'");
        promptPanel.SetActive(evt.Show);
        if (evt.Show)
            promptText.text = evt.PromptText;
    }
}