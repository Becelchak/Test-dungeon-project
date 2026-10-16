using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Stat Modifier")]
public class StatModifierEffect : SkillEffectData
{
    public ModifierTarget target;
    public ModifierOp op;
    public float value;

    private IModifierService _modifierService;

    public override void Apply(ICharacterStatsCalculator c)
    {
        if (_modifierService == null)
        _modifierService = ServiceLocator.Instance.GetService<IModifierService>();

        if (_modifierService != null)
        {
            _modifierService.AddModifier(new StatModifier
            {
                SourceId = name,
                Target = target,
                Op = op,
                Value = value
            });
        }
    }

    public override void Remove(ICharacterStatsCalculator c)
    {
        if (_modifierService != null)
        {
            _modifierService.RemoveModifier(name);
        }
    }
}
