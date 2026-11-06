using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Modify Stat")]
public class ModifyStat : TriggerAction
{
    [SerializeField] private ModifierTarget target;
    [SerializeField] private ModifierOp type;
    [SerializeField] private float value;
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
