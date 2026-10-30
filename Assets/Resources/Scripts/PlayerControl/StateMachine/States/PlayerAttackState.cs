using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private IEquipmentService _equipment;
    private AttackDefinition _currentAttack;
    private float _attackTimer;
    private float _attackDuration;
    private bool _hasAttacked;
    private bool _isWeakAttack;
    private float _attackMoveSpeedMultiplier = 0.5f;

    public PlayerAttackState(PlayerStateMachine stateMachine, IPlayerMovementService movementService, bool isWeakAttack = false)
        : base(stateMachine, movementService)
    {
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _isWeakAttack = isWeakAttack;
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

        // 1. Выбирается атака (клип + тип + вес IK)
        _currentAttack = _stateMachine.playerAnimationController.TriggerRandomAttack(weapon);
        if (_currentAttack == null)
        {
            Debug.LogWarning("[PlayerAttackState] Нет валидной AttackDefinition — прерываем атаку");
            _stateMachine.TransitionToState(new PlayerIdleState(_stateMachine, _movementService));
            return;
        }

        // Проверка AimIKController
        if (_stateMachine.aimIKController == null)
        {
            Debug.LogError("[PlayerAttackState] AimIKController не назначен! Атака без IK.");
        }

        // Активация aim-слой для ОБЕИХ типов:
        // Slash — там работает pitch корпуса, Thrust — position/rotation IK.
        if (_stateMachine.aimIKController != null)
        {
            _stateMachine.aimIKController.SetAttackMode(_currentAttack);
            _stateMachine.aimIKController.SetAimActive(true);
            _stateMachine.aimIKController.SetAttackProgress(0f);
        }

        if (_stateMachine.weaponIKController != null)
            _stateMachine.weaponIKController.SetPose(WeaponIKController.WeaponPose.Attack, instantly: true);

        _stateMachine.CombatService.SetWeaponDamageSource(true, _isWeakAttack);

        _attackDuration = weapon.Stats.attackDuration * (_isWeakAttack ? weapon.Stats.weakAttackSpeedMultiplier : 1f);
        float nativeClipLength = _stateMachine.playerAnimationController.GetCurrentAttackClipLength();
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

        // Пуш прогресса в AimIKController каждый кадр
        float progress = _attackDuration > 0.0001f
            ? Mathf.Clamp01(_attackTimer / _attackDuration)
            : 1f;
        _stateMachine.aimIKController.SetAttackProgress(progress);

        if (!_hasAttacked && _attackTimer > _attackDuration * 0.7f)
            _hasAttacked = true;

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

        var ikController = _stateMachine.weaponIKController;
        var weapon = _equipment.CurrentWeapon;
        var combatService = _stateMachine.CombatService;

        if (ikController != null && weapon != null)
        {
            if (combatService != null && combatService.IsBlocking)
                ikController.SetPose(WeaponIKController.WeaponPose.Block, instantly: true);
            else
                ikController.SetPose(WeaponIKController.WeaponPose.Idle, instantly: false);
        }

        _stateMachine.playerAnimator.SetBool("Block", combatService.IsBlocking);
        combatService.SetWeaponDamageSource(false);

        // Сброс IK-слоя атаки
        _stateMachine.aimIKController.SetAimActive(false);
        _stateMachine.aimIKController.SetAttackMode(null);
        _stateMachine.aimIKController.SetAttackProgress(0f);

        _currentAttack = null;
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

    }

    private void CallMove()
    {
        var stats = _characterStatsService?.CurrentStats;
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