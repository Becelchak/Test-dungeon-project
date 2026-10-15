using UnityEngine;
using System.Collections.Generic;

public class NPCStartDialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcIDString;
    [SerializeField] private string interactionPrompt;
    private bool canInteract = true;
    private IWindowService windowService;

    public Transform Transform => transform;

    void Start()
    {
        windowService = ServiceLocator.Instance.GetService<IWindowService>();
    }

    public bool CanInteract(GameObject interactor) => canInteract;

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact(GameObject interactor, string optionId = "default")
    {
        windowService.ShowWindow<ClassicalDialogueViewModel>(UILayer.Dialogue, (viewModel) =>
        {
            viewModel.Setup(npcIDString);
        });
    }

    public string GetDisplayName() => "NPC";

    public List<InteractionOption> GetOptions()
    {
        return new List<InteractionOption>
        {
            new InteractionOption { Id = "talk", Label = "Поговорить", Hotkey = KeyCode.E }
        };
    }

}
