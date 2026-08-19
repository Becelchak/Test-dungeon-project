using UnityEngine;

/// <summary>
/// Состояние отступления NPC. NPC отбегает от цели на заданную дистанцию,
/// после чего возвращается в обычное поведение.
/// </summary>
public class NpcRetreatState : NpcBaseState
{
    private Vector3 _retreatTarget;
    private float _retreatSpeed;
    private float _stuckTimer;
    private Vector3 _lastPosition;

    public NpcRetreatState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();

        var target = Machine.Perception.CurrentTarget;
        if (target == null)
        {
            Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        Vector3 awayDirection = (Machine.Controller.Transform.position - target.position).normalized;
        awayDirection.y = 0f;
        if (awayDirection.sqrMagnitude < 0.001f)
            awayDirection = Machine.Controller.Transform.forward;

        float distance = Data?.retreatDistance ?? 4f;
        _retreatTarget = Machine.Controller.Transform.position + awayDirection * distance;
        _retreatSpeed = (Data?.moveSpeed ?? 3.5f) * (Data?.retreatSpeedMultiplier ?? 1.3f);
        _stuckTimer = 0f;
        _lastPosition = Machine.Controller.Transform.position;

        Machine.AnimationController?.SetMoving(true);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        Vector3 toTarget = _retreatTarget - Machine.Controller.Transform.position;
        toTarget.y = 0f;

        // Достигли точки отступления
        if (toTarget.sqrMagnitude <= 0.25f)
        {
            Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        // Проверка на застревание
        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= 0.5f)
        {
            float movedSqr = (Machine.Controller.Transform.position - _lastPosition).sqrMagnitude;
            if (movedSqr < 0.01f)
            {
                Machine.TransitionToState(new NpcIdleState(Machine));
                return;
            }
            _lastPosition = Machine.Controller.Transform.position;
            _stuckTimer = 0f;
        }

        MoveToRetreatPoint(toTarget.normalized);
    }

    public override void Exit()
    {
        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
    }

    private void MoveToRetreatPoint(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            Machine.Controller.Transform.rotation = Quaternion.Slerp(
                Machine.Controller.Transform.rotation,
                Quaternion.LookRotation(direction),
                (Data?.rotationSpeed ?? 5f) * Time.deltaTime * 2f
            );
        }

        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
        {
            Machine.Agent.SetDestination(_retreatTarget);
            float speedRatio = Machine.Agent.velocity.magnitude / Mathf.Max(_retreatSpeed, 0.001f);
            Machine.AnimationController?.SetSpeed(Mathf.Clamp01(speedRatio));
        }
        else
        {
            Machine.Controller.Transform.position += direction * _retreatSpeed * Time.deltaTime;
            Machine.AnimationController?.SetSpeed(1f);
        }
    }
}
