using UnityEngine;

public class NPCStartAIDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private IWindowService windowService;
    private AIClient aiService;

    void Start()
    {
        windowService =  ServiceLocator.Instance.GetService<IWindowService>();
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
        windowService.ShowWindow<AIDialogueViewModel>(UILayer.Dialogue, (viewModel) =>
        {
            viewModel.Setup(npcIDString);
        });
    }

}
