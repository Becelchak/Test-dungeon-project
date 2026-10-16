using System;

public interface IModifierService
{
    float GetValue(ModifierTarget key, float baseValue);
    void AddModifier(StatModifier modifier);
    void RemoveModifier(string sourceId);
    void ClearAll();
    bool HasCapability(ModifierTarget capability);
    void RegisterCapability(ModifierTarget capability);
    void UnregisterCapability(ModifierTarget capability);

    event Action OnModifiersChanged;
}
