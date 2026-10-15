using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Actions/Don't Consume Item")]
public class DontConsumeItemAction : TriggerAction
{
    public override void Execute(GameContext ctx, TriggerRuntime runtime)
    {
        if (ctx.Event is ItemUsedEvent itemEvt) itemEvt.Consumed = false;
    }
}
