using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Conditions/From Behind")]
public class FromBehindCondition : TriggerCondition
{
    public override bool Evaluate(GameContext ctx)
    {
        if (ctx.Event is not DamageTakenEvent d || d.Attacker == null) return false;
        Vector3 toAttacker = d.Attacker.transform.position - ctx.Actor.transform.position;
        return Vector3.Dot(ctx.Actor.transform.forward, toAttacker) < -0.3f;
    }
}
