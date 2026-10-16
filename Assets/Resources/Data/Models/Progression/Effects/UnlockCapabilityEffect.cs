using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Unlock Capability")]
public class UnlockCapabilityEffect : SkillEffectData
{
    public ModifierTarget capability;
    private IModifierService modifierService;

    public void Awake()
    {
        modifierService = ServiceLocator.Instance.GetService<IModifierService>();
    }
    public override void Apply(ICharacterStatsCalculator c)
    {
        if (modifierService != null)
        {
            modifierService.AddModifier(new StatModifier
            {
                SourceId = name,
                Target = capability,
                Op = ModifierOp.Override,
                Value = 1f
            });
        }
    }

    public override void Remove(ICharacterStatsCalculator c)
    {
        if (modifierService != null)
        {
            modifierService.RemoveModifier(name);
        }
    }
}
