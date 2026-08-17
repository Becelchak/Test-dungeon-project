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
}
