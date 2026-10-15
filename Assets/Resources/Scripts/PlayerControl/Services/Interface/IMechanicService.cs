using System;
using UnityEditor.EditorTools;
using UnityEngine;

public interface IMechanicService
{
    bool IsActive(string mechanicId);
    int GetValue(string mechanicId);
    MechanicTier GetCurrentTier(string mechanicId);

    void ActivateMechanic(string mechanicId);
    void DeactivateMechanic(string mechanicId);

    void Modify(string mechanicId, int delta, string reason);

    [Tooltip("id, newValue")]
    event Action<string, int> OnMechanicValueChanged;
    [Tooltip("id, newTier")]
    event Action<string, MechanicTier> OnTierChanged;
}