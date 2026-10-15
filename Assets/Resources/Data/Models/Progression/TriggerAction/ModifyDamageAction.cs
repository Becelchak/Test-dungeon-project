using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Modify Damage")]
public class ModifyDamageAction : TriggerAction
{
    [Tooltip("Тип влияния на урон. По умолчанию - процентный")]
    public ModifierType type = ModifierType.Percent;
    public float value = 0f;

    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        switch (type)
        {
            case ModifierType.Add: ctx.FinalResult += value; break;
            case ModifierType.Multiply: ctx.FinalResult *= value; break;
            case ModifierType.Percent: ctx.FinalResult *= (1f + value / 100f); break;
        }
    }
}
