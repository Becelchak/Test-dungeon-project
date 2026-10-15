using System;
using System.Collections.Generic;

public interface ICharacterStatsCalculator
{
    FinalPlayerStats CurrentStats { get; }

    void AddModifier(StatType stat, SkillEffectData source);
    void RemoveModifier(StatType stat, SkillEffectData source);

    void UnlockMechanic(string mechanicId);
    void LockMechanic(string mechanicId);
    void RegisterTrigger(TriggeredEffect trigger);
    void UnregisterTrigger(TriggeredEffect trigger);
    bool HasMechanic(string mechanicId);
    List<SkillEffectData> GetEffectsFor(string sourceId);
    void Recalculate();

    event Action<FinalPlayerStats> OnStatsChanged;
}
