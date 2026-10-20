using System;
using System.Collections.Generic;

public interface ICharacterStatsCalculator
{
    FinalPlayerStats CurrentStats { get; }
    void RegisterTrigger(TriggeredEffect trigger);
    void UnregisterTrigger(TriggeredEffect trigger);
    List<SkillEffectData> GetEffectsFor(string sourceId);
    void Recalculate();
    void SyncCurrentHealthWithMax();

    event Action<FinalPlayerStats> OnStatsChanged;
}
