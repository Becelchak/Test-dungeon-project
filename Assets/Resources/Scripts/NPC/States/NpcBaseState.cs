using UnityEngine;

/// <summary>
/// Базовый класс состояния NPC.
/// </summary>
public abstract class NpcBaseState
{
    protected readonly NpcStateMachine Machine;
    protected readonly NpcData Data;
    protected float TimeEntered;

    public NpcBaseState(NpcStateMachine machine)
    {
        Machine = machine;
        Data = machine.Controller != null ? machine.Controller.Data : null;
    }

    public virtual void Enter()
    {
        TimeEntered = Time.time;
    }

    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }

    /// <summary>
    /// Проверяет, не требуется ли перейти в тактическое состояние (блок, обход, отступление и т.д.).
    /// Возвращает true, если переход выполнен.
    /// </summary>
    protected bool TryEnterTacticalState()
    {
        if (Machine.Tactics == null) return false;

        var tacticalStateType = Machine.Tactics.EvaluateTacticalState();
        if (tacticalStateType == null) return false;

        if (tacticalStateType == typeof(NpcBlockState))
        {
            Machine.TransitionToState(new NpcBlockState(Machine));
            return true;
        }
        if (tacticalStateType == typeof(NpcStrafeState))
        {
            Machine.TransitionToState(new NpcStrafeState(Machine));
            return true;
        }
        if (tacticalStateType == typeof(NpcRetreatState))
        {
            Machine.TransitionToState(new NpcRetreatState(Machine));
            return true;
        }
        if (tacticalStateType == typeof(NpcAggressiveChaseState))
        {
            Machine.TransitionToState(new NpcAggressiveChaseState(Machine));
            return true;
        }

        return false;
    }
}
