using System;
using System.Collections.Generic;
using UnityEngine;

public class MechanicService : BaseService, IMechanicService
{
    private Dictionary<string, MechanicDefinition> _definitions;
    private IPlayerProfileService _profile;
    private ICharacterStatsCalculator _stats;

    public event Action<string, int> OnMechanicValueChanged;
    public event Action<string, MechanicTier> OnTierChanged;

    public void Modify(string mechanicId, int delta, string reason)
    {
        if (!IsActive(mechanicId)) return;

        //var state = GetState(mechanicId);
        var def = _definitions[mechanicId];
        //int old = state.currentValue;

        //state.currentValue = Mathf.Clamp(old + delta, def.minValue, def.maxValue);
        _profile.SaveProfile(_profile.CurrentProfile);

        //OnMechanicValueChanged?.Invoke(mechanicId, state.currentValue);

        // Проверка смены тира
        var newTier = GetCurrentTier(mechanicId);
        //if (newTier != _currentTier) ApplyTierChange(mechanicId, newTier);
    }

    public void ActivateMechanic(string mechanicId)
    {
        //var state = GetOrCreateState(mechanicId);
        //state.isActive = true;
        //ApplyTierChange(mechanicId, GetCurrentTier(mechanicId));
    }

    protected override Type GetServiceType() => typeof(IMechanicService);

    public bool IsActive(string mechanicId)
    {
        throw new NotImplementedException();
    }

    public int GetValue(string mechanicId)
    {
        throw new NotImplementedException();
    }

    public MechanicTier GetCurrentTier(string mechanicId)
    {
        throw new NotImplementedException();
    }

    public void DeactivateMechanic(string mechanicId)
    {
        throw new NotImplementedException();
    }
}