using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    private bool _isTransitioning = false;

    public PlayerIdleState(PlayerStateMachine stateMachine, IPlayerMovementService movementService)
        : base(stateMachine, movementService) { }

    public override void Enter()
    {
        base.Enter();
        _movementService.CalculateMovementDirection();
        _movementService.SetMovement( 0, _playerStats.CurrentProfile.deceleration);
        _stateMachine.playerAnimator.SetBool("IsGrounded", _movementService.CheckGround());
        _stateMachine.playerAnimator.SetBool("Block", _stateMachine.CombatService.IsBlocking);
        _movementService.IsRunning = false;


        var cameraService = ServiceLocator.Instance.GetService<ICameraService>();
        cameraService?.SetOrbitMode();
    }

    public override void Update()
    {
        TryRegenerateStamina();
        
    }

    public override void HandleMoveInput(Vector3 direction)
    {
        // Если получен ввод движения И это не нулевой вектор
        if (direction.magnitude > 0.1f && !_isTransitioning)
        {
            _isTransitioning = true;
            var moveState = new PlayerMoveState(_stateMachine, _movementService, direction);
            _stateMachine.TransitionToState(moveState);
        }
        else
        {
            float deceleration = _equipmentStatsService?.CurrentStats?.Deceleration ?? _playerStats.CurrentProfile.deceleration;
            _movementService._currentSpeed = Mathf.Lerp(_movementService._currentSpeed, 0f, deceleration * Time.deltaTime);
            _movementService.SetMovement(0f, deceleration);
        }
    }

    public override void HandleInteractionInput()
    {
        Debug.Log("INTERACT");
        _stateMachine.interactor.TryInteract();
    }

    public override void HandleBlockInput(bool isBlocking)
    {
        _stateMachine.playerAnimator.SetBool("Block", isBlocking);
    }

    public override void HandleJumpInput(Vector3 direction)
    {
        if (direction.magnitude < 0.5f)
            direction = Vector3.up;
        if (_movementService.CheckGround())
        {
            Debug.Log($"JUMP");
            var jumpState = new PlayerJumpState(_stateMachine, _movementService, direction);
            _stateMachine.TransitionToState(jumpState);
        }
    }

    public override void HandleAttackInput()
    {
        if (CanAttackFromIdle())
        {
            Debug.Log($"ATTACK");
            var attackState = new PlayerAttackState(_stateMachine, _movementService);
            _stateMachine.TransitionToState(attackState);
        }
    }

    private void TryRegenerateStamina()
    {
        // Пример: восстановление стамины со временем
        _playerStats.CurrentProfile.health = (int)Mathf.Floor(
            Mathf.Min(_playerStats.CurrentProfile.maxHealth,
            _playerStats.CurrentProfile.health + Time.deltaTime * _playerStats.CurrentProfile.healthRegenRate)
            );
    }

    private bool CanAttackFromIdle()
    {
        // Проверяем условия для атаки:
        // 1. Есть ли оружие в руках?
        // 2. Не перезаряжается ли оружие?
        // 3. Хватит ли стамины?
        // Можно получать доступ к сервису экипировки через ServiceLocator

        //var equipmentService = ServiceLocator.Instance.GetService<IEquipmentService>();
        //return equipmentService != null && equipmentService.HasWeaponEquipped();
        return _playerStats.CurrentProfile.health > 0;
    }

    public override void Exit()
    {
        base.Exit();
        _isTransitioning = false;
    }
}