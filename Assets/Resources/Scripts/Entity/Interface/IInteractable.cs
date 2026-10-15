using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    Transform Transform { get; }
    void Interact(GameObject interactor, string optionId = "default");
    bool CanInteract(GameObject interactor);
    string GetInteractionPrompt();
    string GetDisplayName();
    List<InteractionOption> GetOptions();
}