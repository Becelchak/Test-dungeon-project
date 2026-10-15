using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Restore Stat")]
public class RestoreStatAction : TriggerAction
{
    public StatType stat;
    public float value;
    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        var profile = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        if (stat == StatType.Mana) profile.ModifyMana(Mathf.RoundToInt(value));
        if (stat == StatType.Health) profile.ModifyHealth(Mathf.RoundToInt(value));
        if (stat == StatType.Stamina) profile.ModifyStamina(value);
    }
}

