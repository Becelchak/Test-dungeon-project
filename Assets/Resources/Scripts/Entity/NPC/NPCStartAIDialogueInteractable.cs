using UnityEngine;

public class NPCStartAIDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private WindowService windowService;
    private AIClient aiService;

    void Start()
    {
        windowService = (WindowService) ServiceLocator.Instance.GetService<IWindowService>();
        aiService = (AIClient) ServiceLocator.Instance.GetService<IAIService>();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (canInteract && aiService.IsConnected)
            return true;
        else return false;
    }

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor)
    {
        windowService.ShowAIDialogue(npcIDString);
    }

}
