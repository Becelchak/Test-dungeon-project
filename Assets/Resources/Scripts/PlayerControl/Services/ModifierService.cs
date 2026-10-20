using System;
using System.Collections.Generic;
using UnityEngine;

public class ModifierService : BaseService, IModifierService
{
    private Dictionary<ModifierTarget, List<StatModifier>> _statModifiers = new Dictionary<ModifierTarget, List<StatModifier>>();
    private HashSet<ModifierTarget> _capabilities = new HashSet<ModifierTarget>();

    public event Action OnModifiersChanged;

    protected override Type GetServiceType() => typeof(IModifierService);

    public float GetValue(ModifierTarget key, float baseValue)
    {
        if (!_statModifiers.ContainsKey(key) || _statModifiers[key].Count == 0)
            return baseValue;

        float result = baseValue;

        foreach (var modifier in _statModifiers[key])
        {
            switch (modifier.Op)
            {
                case ModifierOp.Add:
                    result += modifier.Value;
                    break;
                case ModifierOp.Multiply:
                    result *= modifier.Value;
                    break;
                case ModifierOp.Override:
                    result = modifier.Value;
                    break;
                case ModifierOp.Percent:
                    result *= (1f + modifier.Value / 100f);
                    if (result <= 0f)
                        result = modifier.Value / 100f;
                    break;
            }
        }

        return result;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (!_statModifiers.ContainsKey(modifier.Target))
        {
            _statModifiers[modifier.Target] = new List<StatModifier>();
        }

        _statModifiers[modifier.Target].Add(modifier);
        OnModifiersChanged?.Invoke();
        Debug.Log($"[ModifierService] Добавлен модификатор: {modifier.SourceId} -> {modifier.Target} ({modifier.Op}, {modifier.Value})");
    }

    public void RemoveModifier(string sourceId)
    {
        foreach (var kvp in _statModifiers)
        {
            kvp.Value.RemoveAll(m => m.SourceId == sourceId);
        }

        OnModifiersChanged?.Invoke();
        Debug.Log($"[ModifierService] Удалены все модификаторы от источника: {sourceId}");
    }

    public void ClearAll()
    {
        _statModifiers.Clear();
        _capabilities.Clear();
        OnModifiersChanged?.Invoke();
        Debug.Log("[ModifierService] Все модификаторы очищены");
    }

    public bool HasCapability(ModifierTarget capability)
    {
        return _capabilities.Contains(capability);
    }

    public void RegisterCapability(ModifierTarget capability)
    {
        if (_capabilities.Add(capability))
        {
            Debug.Log($"[ModifierService] Capability зарегистрирована: {capability}");
            OnModifiersChanged?.Invoke();
        }
    }

    public void UnregisterCapability(ModifierTarget capability)
    {
        if (_capabilities.Remove(capability))
        {
            Debug.Log($"[ModifierService] Capability удалена: {capability}");
            OnModifiersChanged?.Invoke();
        }
    }
}
