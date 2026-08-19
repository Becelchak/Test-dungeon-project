/// <summary>
/// Состояние покоя NPC. Ожидает, пока игрок попадёт в поле зрения.
/// </summary>
public class NpcIdleState : NpcBaseState
{
    public NpcIdleState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();
        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;
        if (!Machine.Perception.HasTarget) return;

        if (TryEnterTacticalState()) return;

        if (Machine.Perception.IsTargetInAttackRange)
        {
            if (Machine.Combat.CanAttack())
                Machine.TransitionToState(new NpcAttackState(Machine));
        }
        else
        {
            Machine.TransitionToState(new NpcChaseState(Machine));
        }
    }
}
