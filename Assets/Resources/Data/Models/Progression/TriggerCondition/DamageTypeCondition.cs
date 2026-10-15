using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Conditions/Damage Type")]
public class DamageTypeCondition : TriggerCondition
{
    public DamageType requiredType;
    public override bool Evaluate(GameContext ctx) =>
        ctx.Event is DamageTakenEvent d && d.Type == requiredType;
}
