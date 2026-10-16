using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Modify Damage")]
public class ModifyDamageAction : TriggerAction
{
    [Tooltip("Тип влияния на урон. По умолчанию - процентный")]
    public ModifierOp type = ModifierOp.Percent;
    public float value = 0f;

    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        switch (type)
        {
            case ModifierOp.Add: ctx.FinalResult += value; break;
            case ModifierOp.Multiply: ctx.FinalResult *= value; break;
            case ModifierOp.Percent: ctx.FinalResult *= (1f + value / 100f); break;
        }
    }
}
