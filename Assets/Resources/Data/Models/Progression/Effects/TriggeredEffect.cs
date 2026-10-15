using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Triggered")]
public class TriggeredEffect : SkillEffectData
{
    [Header("Identity")]
    public string triggerId;
    public string displayName;

    [Header("Match")]
    [Tooltip("Если пусто — сработает на любое событие, но conditions отфильтруют")]
    public GameEventCategory category;
    public List<TriggerCondition> conditions;

    [Header("Roll")]
    [Range(0f, 1f)] public float chance = 1f;

    [Header("Actions")]
    public List<TriggerAction> actions;

    [Header("Limits")]
    public float cooldown = 0f;
    [Tooltip("Максимальное число срабатываний. При значении -1, лимита нет")]
    public int maxPerEncounter = -1;
    public override void Apply(ICharacterStatsCalculator c) => c.RegisterTrigger(this);
    public override void Remove(ICharacterStatsCalculator c) => c.UnregisterTrigger(this);
}
