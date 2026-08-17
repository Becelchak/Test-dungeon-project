using UnityEngine;

/// <summary>
/// Состояние преследования. Заглушка: поворачивается к цели и движется к ней
/// (через NavMeshAgent, если есть, иначе просто вперёд).
/// </summary>
public class NpcChaseState : NpcBaseState
{
    private float _targetLostTimer;

    public NpcChaseState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();
        Machine.AnimationController?.SetMoving(true);
        _targetLostTimer = 0f;
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        if (!Machine.Perception.HasTarget)
        {
            _targetLostTimer += Time.deltaTime;
            if (_targetLostTimer >= (Data?.targetLostDelay ?? 3f))
                Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        _targetLostTimer = 0f;

        if (Machine.Perception.IsTargetInAttackRange && Machine.Combat.CanAttack())
        {
            Machine.TransitionToState(new NpcAttackState(Machine));
            return;
        }

        MoveTowardTarget();
    }

    public override void Exit()
    {
        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
            Machine.Agent.ResetPath();

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
                (Data?.rotationSpeed ?? 5f) * Time.deltaTime
            );
        }

        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
        {
            Machine.Agent.SetDestination(target.position);
            float speedRatio = Machine.Agent.velocity.magnitude / Mathf.Max(Data?.moveSpeed ?? 1f, 0.001f);
            Machine.AnimationController?.SetSpeed(Mathf.Clamp01(speedRatio));
        }
        else
        {
            // Fallback: прямолинейное движение, если NavMeshAgent отсутствует
            Machine.Controller.Transform.position += direction.normalized * (Data?.moveSpeed ?? 0f) * Time.deltaTime;
            Machine.AnimationController?.SetSpeed(1f);
        }
    }
}
