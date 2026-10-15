using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/ActionEffect")]
public class ActionEffect : SkillEffectData
{
    public StatType stat;
    public ActionType action;
    public override void Apply(ICharacterStatsCalculator calc)
    {
        switch (action)
        {
            case ActionType.Cancel:
                calc.RemoveModifier(stat, this);
                break;
            case ActionType.Apply:
                calc.AddModifier(stat, this);
                break;
            case ActionType.Repeat:
                calc.AddModifier(stat, this);
                break;
        }
    }

    public override void Remove(ICharacterStatsCalculator calc)
    {
        calc.RemoveModifier(stat, this);
    }
}
