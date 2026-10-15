using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerService : BaseService, ITriggerService
{
    [SerializeField] private TriggerDatabase database;
    private Dictionary<string,TriggeredEffect> allTriggers;
    private readonly Dictionary<string, TriggerRuntime> _runtimes = new();
    private List<TriggeredEffect> _triggers = new();
    public event Action<TriggeredEffect, GameContext> OnTriggerFired;
    private IPlayerProfileService _profileService;
    private ICharacterStatsCalculator _characterStatsCalculator;

    protected override Type GetServiceType() => typeof(ITriggerService);

    public void Start()
    {
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _characterStatsCalculator = ServiceLocator.Instance.GetService<ICharacterStatsCalculator>();
        if (_characterStatsCalculator != null)
            _characterStatsCalculator.OnStatsChanged += SyncWithProfile;
        LoadTriggers();
    }

    private void LoadTriggers()
    {
        if (database != null)
        {
            
            allTriggers = database.Database;
            
            Debug.Log("[TriggerService] Синхронизация с Trigger Database успешна!");
        }
        else
        {
            Debug.Log("[TriggerService] Отсутствует ссылка на Trigger Database!");
            return;
        }

        var profile = _profileService.CurrentProfile;
        var desired = new HashSet<string>();

        CollectTriggerIds(profile.activeTraitIds, desired);
        CollectTriggerIds(profile.unlockedSkillIds, desired);

        foreach (var id in desired)
            if (allTriggers.TryGetValue(id, out var trigger))
                Register(trigger);
    }

    private void SyncWithProfile(FinalPlayerStats stats)
    {
        var profile = _profileService.CurrentProfile;
        var desired = new HashSet<string>();

        CollectTriggerIds(profile.activeTraitIds, desired);
        CollectTriggerIds(profile.unlockedSkillIds, desired);

        foreach (var id in desired)
            if (allTriggers.TryGetValue(id, out var trigger))
                Register(trigger);
    }

    private void CollectTriggerIds(List<string> sourceIds, HashSet<string> output)
    {
        foreach (var sourceId in sourceIds) 
        {
            var effects = _characterStatsCalculator.GetEffectsFor(sourceId);
            if (effects == null) continue;
            foreach(var e in effects)
                if (e is TriggeredEffect t) output.Add(t.triggerId);
        }
    }

    public void Register(TriggeredEffect trigger)
    {
        if (_runtimes.ContainsKey(trigger.triggerId)) return;
        _runtimes[trigger.triggerId] = new TriggerRuntime { Source = trigger };
        _triggers.Add(trigger);
    }

    public void Unregister(TriggeredEffect trigger)
    {
        if (_runtimes.Remove(trigger.triggerId))
            _triggers.Remove(trigger);
    }

    public void Dispatch(GameEvent evt, GameObject actor)
    {
        var ctx = new GameContext(evt, actor);
        if (evt is DamageTakenEvent dmg) ctx.FinalResult = dmg.Amount;

        foreach (var trigger in _triggers.ToArray())
        {
            if (trigger.category != evt.Category) continue;

            var runTime = _runtimes[trigger.triggerId];
            if (runTime.IsOnCooldown || runTime.ExceededMaxUses) continue;

            bool pass = true;
            foreach (var cond in trigger.conditions)
                if (!cond.Evaluate(ctx)) { pass = false; break; }
            if (!pass) continue;

            if (trigger.chance < 1f && UnityEngine.Random.value > trigger.chance) continue;

            foreach (var action in trigger.actions)
            {
                action.Execute(ctx, runTime);
                Debug.Log($"ВЫПОЛНИЛОСЬ СОБЫТИЕ ТРИГГЕРА {trigger.triggerId}");
            }

            runTime.MarkUsed();
            OnTriggerFired?.Invoke(trigger, ctx);

            //Если триггер отменяет событие - прервать его выполнение
            if (ctx.IsCancelled) break;
        }

        // Отправка финального значения обратно в событие, если оно гибкое
        if (evt is DamageTakenEvent damageTaken)
            damageTaken.Amount = ctx.FinalResult;
    }

    public void ResetEncounter()
    {
        foreach (var rt in _runtimes.Values) rt.ResetForEncounter();
    }
}