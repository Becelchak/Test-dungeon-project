using UnityEngine;

/// <summary>
/// Состояние агрессивного преследования NPC. NPC движется быстрее обычного
/// и чаще пытается атаковать, когда у игрока мало здоровья.
/// </summary>
public class NpcAggressiveChaseState : NpcBaseState
{
    private float _originalSpeed;
    private float _aggressiveSpeed;

    public NpcAggressiveChaseState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();

        _originalSpeed = Machine.Agent != null ? Machine.Agent.speed : (Data?.moveSpeed ?? 3.5f);
        _aggressiveSpeed = _originalSpeed * (Data?.aggressiveSpeedMultiplier ?? 1.4f);

        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
            Machine.Agent.speed = _aggressiveSpeed;

        Machine.AnimationController?.SetMoving(true);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        if (!Machine.Perception.HasTarget)
        {
            Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        if (Machine.Perception.IsTargetInAttackRange && Machine.Combat.CanAttack())
        {
            Machine.TransitionToState(new NpcAttackState(Machine));
            return;
        }

        MoveTowardTarget();

        // Если игрок восстановил здоровье — прекращаем агрессию
        if (Machine.Tactics.PlayerHealthPercent > Data?.aggressivePlayerHealthThreshold)
        {
            Machine.TransitionToState(new NpcChaseState(Machine));
        }
    }

    public override void Exit()
    {
        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
            Machine.Agent.speed = _originalSpeed;

        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
    }

    private void MoveTowardTarget()
    {
        var target = Machine.Perception.CurrentTarget;
        if (target == null) return;

        Vector3 targetPosition = target.position;
        targetPosition.y = Machine.Controller.Transform.position.y;

        Vector3 direction = targetPosition - Machine.Controller.Transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Machine.Controller.Transform.rotation = Quaternion.Slerp(
                Machine.Controller.Transform.rotation,
                lookRotation,
                (Data?.rotationSpeed ?? 5f) * Time.deltaTime * 1.5f
            );
        }

        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
        {
            Machine.Agent.SetDestination(target.position);
            float speedRatio = Machine.Agent.velocity.magnitude / Mathf.Max(_aggressiveSpeed, 0.001f);
            Machine.AnimationController?.SetSpeed(Mathf.Clamp01(speedRatio));
        }
        else
        {
            Machine.Controller.Transform.position += direction.normalized * _aggressiveSpeed * Time.deltaTime;
            Machine.AnimationController?.SetSpeed(1f);
        }
    }
}
