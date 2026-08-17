using UnityEngine;

/// <summary>
/// Состояние атаки NPC. Проигрывает анимацию и активирует хитбокс оружия в нужный момент.
/// </summary>
public class NpcAttackState : NpcBaseState
{
    private float _timer;

    public NpcAttackState(NpcStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        base.Enter();
        _timer = 0f;

        Machine.Agent?.ResetPath();
        Machine.AnimationController?.SetMoving(false);
        Machine.AnimationController?.TriggerAttack();
        Machine.Combat?.StartAttack(Data.attackDamage, Data.attackDuration, Data.attackWindup);
    }

    public override void Update()
    {
        if (!Machine.Controller.IsAlive) return;

        _timer += Time.deltaTime;
        FaceTarget();

        if (_timer >= Data.attackDuration)
        {
            // По окончании атаки возвращаемся в Idle — оно само решит, преследовать или атаковать снова
            Machine.TransitionToState(new NpcIdleState(Machine));
        }
    }

    public override void Exit()
    {
        Machine.Combat?.EndAttack();
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
                Data.rotationSpeed * Time.deltaTime * 2f
            );
        }
    }
}
