using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Unlock Capability")]
public class UnlockCapabilityEffect : SkillEffectData
{
    public ModifierTarget capability;
    private IModifierService modifierService;
    public override void Apply(ICharacterStatsCalculator c)
    {
        modifierService = ServiceLocator.Instance.GetService<IModifierService>();
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
        else
        {
            Debug.LogError("[UnlockCapabilityEffect] IModifierService not found"); 
            return;
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
