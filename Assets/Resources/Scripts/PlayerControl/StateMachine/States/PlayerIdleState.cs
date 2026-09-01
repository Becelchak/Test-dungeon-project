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

    public override void HandleDodgeInput(Vector3 direction)
    {
        _currentInput = Vector3.Lerp(_currentInput,
direction,
_playerStats.CurrentProfile.acceleration * Time.deltaTime);
        _stateMachine.playerAnimator.SetTrigger("Dodge");
        _playerStats.ModifyStamina((int)_playerStats.CurrentProfile.dodgeCost);
        _stateMachine.CombatService.SetGodMode(true);


        _movementService.UpdateMovementInput(direction);
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

    public override void HandleParryInput()
    {
        // Парирование обрабатывается централизованно в PlayerStateMachine.
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
        if (!CanAttackFromIdle()) return;

        var combatService = _stateMachine.CombatService;
        if (combatService == null) return;

        if (combatService.TryStartAttack(out bool isWeakAttack))
        {
            Debug.Log($"ATTACK (weak={isWeakAttack})");
            var attackState = new PlayerAttackState(_stateMachine, _movementService, isWeakAttack);
            _stateMachine.TransitionToState(attackState);
        }
    }

    private void TryRegenerateStamina()
    {
        var profile = _playerStats.CurrentProfile;
        if (profile == null) return;

        int regen = Mathf.RoundToInt(Time.deltaTime * profile.staminaRegenRate);
        if (regen > 0 && profile.stamina < profile.maxStamina)
            _playerStats.ModifyStamina(regen);
        Debug.Log($"Теущая стамина: {_playerStats.CurrentProfile.stamina}");
    }

    private bool CanAttackFromIdle()
    {
        return _stateMachine.CombatService != null && !_stateMachine.CombatService.IsDead;
    }

    public override void Exit()
    {
        base.Exit();
        _isTransitioning = false;
    }
}