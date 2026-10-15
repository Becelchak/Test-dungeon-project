using UnityEngine;

public abstract class TriggerAction : ScriptableObject
{
    public abstract void Execute(GameContext ctx, TriggerRuntime runtime);
}
