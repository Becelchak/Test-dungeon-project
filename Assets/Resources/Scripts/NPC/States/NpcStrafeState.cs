using UnityEngine;

/// <summary>
/// Состояние обхода NPC. Движется по дуге вокруг цели, пытаясь избежать атаки игрока.
/// </summary>
public class NpcStrafeState : NpcBaseState
{
    private float _timer;
    private float _duration;
    private float _strafeSign;
    private float _strafeSpeed;

    public NpcStrafeState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();

        _timer = 0f;
        _duration = Data?.strafeDuration ?? 1f;
        _strafeSign = Random.value > 0.5f ? 1f : -1f;
        _strafeSpeed = (Data?.moveSpeed ?? 3.5f) * (Data?.strafeSpeedMultiplier ?? 0.9f);

        Machine.AnimationController?.SetMoving(true);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        _timer += Time.deltaTime;

        var target = Machine.Perception.CurrentTarget;
        if (target == null || _timer >= _duration)
        {
            Machine.TransitionToState(new NpcIdleState(Machine));
            return;
        }

        StrafeAroundTarget(target);
    }

    public override void Exit()
    {
        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
    }

    private void StrafeAroundTarget(Transform target)
    {
        Vector3 toTarget = target.position - Machine.Controller.Transform.position;
        toTarget.y = 0f;
        float currentDistance = toTarget.magnitude;

        // Направление обхода — перпендикулярно вектору на цель
        Vector3 strafeDirection = Vector3.Cross(toTarget.normalized, Vector3.up) * _strafeSign;

        // Корректировка радиуса: если далеко — подойти ближе, если близко — отодвинуться
        float desiredRadius = Data?.strafeRadius ?? 2.5f;
        Vector3 radiusCorrection = (currentDistance < desiredRadius ? -toTarget.normalized : toTarget.normalized) * 0.5f;

        Vector3 moveDirection = (strafeDirection + radiusCorrection).normalized;

        // Поворот к цели
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Machine.Controller.Transform.rotation = Quaternion.Slerp(
                Machine.Controller.Transform.rotation,
                Quaternion.LookRotation(toTarget.normalized),
                (Data?.rotationSpeed ?? 5f) * Time.deltaTime * 1.5f
            );
        }

        if (Machine.Agent != null && Machine.Agent.isActiveAndEnabled)
        {
            Vector3 destination = Machine.Controller.Transform.position + moveDirection * _strafeSpeed * Time.deltaTime;
            Machine.Agent.SetDestination(destination);
            float speedRatio = Machine.Agent.velocity.magnitude / Mathf.Max(_strafeSpeed, 0.001f);
            Machine.AnimationController?.SetSpeed(Mathf.Clamp01(speedRatio));
        }
        else
        {
            Machine.Controller.Transform.position += moveDirection * _strafeSpeed * Time.deltaTime;
            Machine.AnimationController?.SetSpeed(1f);
        }
    }
}
