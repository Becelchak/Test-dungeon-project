using UnityEngine;

/// <summary>
/// Состояние блока NPC. NPC поворачивается к цели, уменьшает получаемый урон
/// и выходит из состояния, когда игрок перестаёт атаковать или таймер блока истёк.
/// </summary>
public class NpcBlockState : NpcBaseState
{
    private float _timer;
    private float _maxDuration;

    public NpcBlockState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();

        if (Machine.Defense == null || !Machine.Defense.TryStartBlock())
        {
            Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        _timer = 0f;
        _maxDuration = Data?.blockDuration ?? 1.5f;

        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
        Machine.AnimationController?.SetBlocking(true);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        _timer += Time.deltaTime;
        Machine.Defense?.TickBlock();
        FaceTarget();

        // Выходим из блока, если он завершился или игрок уже давно не атакует
        if (!Machine.Defense.IsBlocking || _timer >= _maxDuration || !Machine.Tactics.IsPlayerAttacking)
        {
            Machine.Defense?.EndBlock();
            Machine.TransitionToState(new NpcIdleState(Machine));
        }
    }

    public override void Exit()
    {
        Machine.Defense?.EndBlock();
        Machine.AnimationController?.SetBlocking(false);
    }

    private void FaceTarget()
    {
        var target = Machine.Perception.CurrentTarget;
        if (target == null) return;

        Vector3 direction = target.position - Machine.Controller.Transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Machine.Controller.Transform.rotation = Quaternion.Slerp(
                Machine.Controller.Transform.rotation,
                Quaternion.LookRotation(direction),
                (Data?.rotationSpeed ?? 5f) * Time.deltaTime * 2f
            );
        }
    }
}
