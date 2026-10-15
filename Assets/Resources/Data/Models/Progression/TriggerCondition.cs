using UnityEngine;

public abstract class TriggerCondition : ScriptableObject
{
    public abstract bool Evaluate(GameContext ctx);
}