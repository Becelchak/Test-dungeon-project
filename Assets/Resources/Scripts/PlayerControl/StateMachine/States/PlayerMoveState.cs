using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class PlayerMoveState : PlayerStateBase
{
    private const float BLOCK_MOVE_MULTIPLIER = 0.8f;
    private float speedMove;
    private bool _isBlocking;

    public PlayerMoveState(PlayerStateMachine stateMachine, IPlayerMovementService movementService, Vector3 direction) : base(stateMachine, movementService)
    {}

    public override void Enter()
    {
        base.Enter();
        _isBlocking = _stateMachine.CombatService.IsBlocking;
        _stateMachine.playerAnimator.SetBool("Block", _isBlocking);
        speedMove = _playerStats.CurrentProfile.speedMove;
        var cameraService = ServiceLocator.Instance.GetService<ICameraService>();
        cameraService?.SetFollowMode();
        Vector2 input = _inputService.GetMovementInput();
        _movementService.UpdateMovementInput(input);
        _movementService.IsRunning = false;
    }

    public override void Update()
    {
        
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        CallMove();


        var moveService = (PlayerMovementService)_movementService;
    }

    public void CallMove()
    {
        var stats = _equipmentStatsService?.CurrentStats;
        if (stats == null)
        {
            _movementService.SetMovement(0f, 0f);
            return;
        }

        float targetSpeed = _movementService.IsRunning ? stats.RunSpeed : stats.MoveSpeed;
        if (_isBlocking)
            targetSpeed *= BLOCK_MOVE_MULTIPLIER;
        float acceleration = stats.Acceleration;

        _movementService.SetMovement(targetSpeed, acceleration);
    }

    public override void HandleMoveInput(Vector3 direction)
    {

        _currentInput = Vector3.Lerp(_currentInput, 
            direction, 
            _playerStats.CurrentProfile.acceleration * Time.deltaTime);
        _movementService.UpdateMovementInput(direction);
        CallMove();

        if (direction.magnitude < 0.1f)
        {
            var idleState = new PlayerIdleState(_stateMachine, _movementService);
            _stateMachine.TransitionToState(idleState);
        }
    }

    public override void HandleJumpInput(Vector3 direction)
    {
        if (_movementService.CheckGround()) // можно прыгать только с земли
        {
            var jumpState = new PlayerJumpState(_stateMachine, _movementService, direction);
            _stateMachine.TransitionToState(jumpState);
        }
    }

    public override void HandleSprintInput(bool sprintInpitPressed)
    {
        _movementService.IsRunning = sprintInpitPressed;
    }

    public override void HandleBlockInput(bool isBlocking)
    {
        _isBlocking = isBlocking;
        _stateMachine.playerAnimator.SetBool("Block", isBlocking);
        CallMove();
    }

    public override void HandleInteractionInput()
    {
        Debug.Log("MOVE INTERACT");
        _stateMachine.interactor.TryInteract();
    }

    public override void HandleMovement(Vector3 direction)
    {
        _currentInput = direction;
        _movementService.UpdateMovementInput(direction);

        if (direction.magnitude > 0.1f)
        {
            CallMove();
        }
        else
        {
            var idleState = new PlayerIdleState(_stateMachine, _movementService);
            _stateMachine.TransitionToState(idleState);
        }
    }

    public override void HandleAttackInput()
    {
        var attackState = new PlayerAttackState(_stateMachine, _movementService);
        _stateMachine.TransitionToState(attackState);
    }
}
