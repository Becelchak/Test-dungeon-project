using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Cancel Event")]
public class CancelEventAction : TriggerAction
{
    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        ctx.IsCancelled = true;
        ctx.FinalResult = 0f;
    }
}