using UnityEngine;
using System.Collections.Generic;

public class NPCStartAIDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private IWindowService windowService;
    private AIClient aiService;

    public Transform Transform => transform;

    void Start()
    {
        windowService = ServiceLocator.Instance.GetService<IWindowService>();
        aiService = (AIClient) ServiceLocator.Instance.GetService<IAIService>();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (canInteract && aiService.IsConnected)
            return true;
        else return false;
    }

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor, string optionId = "default")
    {
        windowService.ShowWindow<AIDialogueViewModel>(UILayer.Dialogue, (viewModel) =>
        {
            viewModel.Setup(npcIDString);
        });
    }

    public string GetDisplayName() => "AI NPC";

    public List<InteractionOption> GetOptions()
    {
        return new List<InteractionOption>
        {
            new InteractionOption { Id = "talk", Label = "Поговорить", Hotkey = KeyCode.E }
        };
    }

}
