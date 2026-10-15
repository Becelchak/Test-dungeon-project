using System;
using UnityEngine;

public interface IInteractionTargetingService
{
    IInteractable CurrentTarget { get; }
    Vector3 TargetWorldPosition { get; }

    event Action<IInteractable> OnTargetChanged;
    event Action OnTargetLost;
}
