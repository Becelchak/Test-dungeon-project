using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Stat Modifier")]
public class StatModifierEffect : SkillEffectData
{
    public StatType stat;
    public ModifierType type;
    public float value;
    public override void Apply(ICharacterStatsCalculator c) => c.AddModifier(stat, this);
    public override void Remove(ICharacterStatsCalculator c) => c.RemoveModifier(stat, this);
}
