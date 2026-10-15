using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Conditions/Health Below %")]
public class HealthBelowCondition : TriggerCondition
{
    [Range(0f, 1f)] public float threshold = 0.5f;
    public override bool Evaluate(GameContext ctx)
    {
        var profile = ServiceLocator.Instance.GetService<IPlayerProfileService>().CurrentProfile;
        return (float)profile.health / profile.maxHealth < threshold;
    }
}
