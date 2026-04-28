using UnityEngine;

public class NPCStartDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private WindowService windowService;

    void Start()
    {
        windowService = (WindowService) ServiceLocator.Instance.GetService<IWindowService>();
    }

    public bool CanInteract(GameObject interactor) => canInteract;

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor)
    {
        windowService.ShowClassicalDialogue(npcIDString);
    }

}
