using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Conditions/Stat Below %")]
public class StatBelowCondition : TriggerCondition
{
    [Range(0f, 1f)] public float threshold = 0.5f;
    [SerializeField] private ModifierTarget target;
    public override bool Evaluate(GameContext ctx)
    {
        var profile = ServiceLocator.Instance.GetService<IPlayerProfileService>().CurrentProfile;
        switch (target)
        {
            case ModifierTarget.CurrentHealth:
                return (float)profile.health / profile.maxHealth < threshold;
            case ModifierTarget.CurrentMana:
                return (float)profile.mana / profile.maxMana < threshold;
            case ModifierTarget.CurrentStamina:
                return (float)profile.stamina / profile.maxStamina < threshold;
            case ModifierTarget.CurrentSanity:
                //return (float)profile.stamina / profile.maxStamina < threshold;
                break;
        }
        return false;
    }
}
