using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Restore Stat")]
public class RestoreStatAction : TriggerAction
{
    public ModifierTarget stat;
    public float value;
    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        var profile = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        if (stat == ModifierTarget.MaxMana) profile.ModifyMana(Mathf.RoundToInt(value));
        if (stat == ModifierTarget.MaxHealth) profile.ModifyHealth(Mathf.RoundToInt(value));
        if (stat == ModifierTarget.MaxStamina) profile.ModifyStamina(value);
    }
}

