using UnityEngine;

public class NPCStartDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private IWindowService windowService;

    void Start()
    {
        windowService = ServiceLocator.Instance.GetService<IWindowService>();
    }

    public bool CanInteract(GameObject interactor) => canInteract;

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor)
    {
        windowService.ShowWindow<ClassicalDialogueViewModel>(UILayer.Dialogue, (viewModel) =>  
        {
            viewModel.Setup(npcIDString);
        });
    }

}
