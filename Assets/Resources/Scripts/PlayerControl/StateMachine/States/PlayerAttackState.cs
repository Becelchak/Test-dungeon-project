using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private IEquipmentService _equipment;
    private float _attackTimer;
    private float _attackDuration;
    private bool _hasAttacked;
    private float _attackMoveSpeedMultiplier = 0.5f;

    public PlayerAttackState(PlayerStateMachine stateMachine, IPlayerMovementService movementService)
        : base(stateMachine, movementService)
    {
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
    }

    public override void Enter()
    {
        base.Enter();
        var weapon = _equipment.CurrentWeapon;
        if (weapon == null)
        {
            Debug.LogError("[PlayerAttackState] Нет текущего оружия!");
            _stateMachine.TransitionToState(new PlayerIdleState(_stateMachine, _movementService));
            return;
        }

        _attackDuration = weapon.Stats.attackDuration;
        var ikController = _stateMachine.weaponIKController;

        if (ikController != null)
        {
            // Переключаем двуручное оружие в режим атаки:
            // оружие возвращается в иерархию правой руки, левая рука остается на хвате.
            ikController.SetAttackMode(true, instantly: true);
        }

        _stateMachine.playerAnimationController.TriggerRandomAttack(weapon);

        float nativeClipLength = _stateMachine.playerAnimationController.GetCurrentAttackClipLength();

        // Высчитываем нужную скорость: если клип 3 сек, а мы хотим 1.5 сек, скорость должна быть 3 / 1.5 = 2.0
        float calculatedSpeed = nativeClipLength / Mathf.Max(_attackDuration, 0.001f);

        _stateMachine.playerAnimator.SetFloat("AttackDuraction", calculatedSpeed);
        _stateMachine.playerAnimator.SetTrigger("Attack");
        _stateMachine.playerAnimator.SetInteger("WeaponType", (int)weapon.weaponType);
        _stateMachine.playerAnimator.SetBool("Block", false);
        _attackTimer = 0f;
        _hasAttacked = false;
    }

    public override void Update()
    {
        _attackTimer += Time.deltaTime;
        if (!_hasAttacked && _attackTimer > _attackDuration * 0.7f)
        {
            _hasAttacked = true;
            // Здесь можно вызвать событие нанесения урона
        }
        if (_attackTimer >= _attackDuration)
        {
            Vector2 input = _inputService.GetMovementInput();
            if (input.magnitude > 0.1f)
                _stateMachine.TransitionToState(new PlayerMoveState(_stateMachine, _movementService, input));
            else
                _stateMachine.TransitionToState(new PlayerIdleState(_stateMachine, _movementService));
        }
    }

    public override void Exit()
    {
        base.Exit();

        // Возвращаем двуручное оружие в IDLE-родителя и восстанавливаем веса хвата
        var ikController = _stateMachine.weaponIKController;
        var weapon = _equipment.CurrentWeapon;
        if (ikController != null && weapon != null)
        {
            ikController.SetAttackMode(false, instantly: false);
        }

        // Восстанавливаем состояние блока, если кнопка всё ещё удерживается
        _stateMachine.playerAnimator.SetBool("Block", _stateMachine.CombatService.IsBlocking);
    }

    public override void HandleMovement(Vector3 direction)
    {
        _currentInput = direction;
        _movementService.UpdateMovementInput(direction);
        CallMove();
    }

    public override void HandleMoveInput(Vector3 direction)
    {
        _currentInput = direction;
        _movementService.UpdateMovementInput(direction);
        CallMove();
    }

    public override void HandleAttackInput()
    {
        // Можно разрешить комбо-атаки, если анимация уже близка к завершению
    }

    private void CallMove()
    {
        var stats = _equipmentStatsService?.CurrentStats;
        if (stats == null)
        {
            _movementService.SetMovement(0f, 0f);
            return;
        }

        float targetSpeed = _movementService.IsRunning ? stats.RunSpeed : stats.MoveSpeed;
        targetSpeed *= _attackMoveSpeedMultiplier;

        float acceleration = stats.Acceleration;
        _movementService.SetMovement(targetSpeed, acceleration);
    }
}