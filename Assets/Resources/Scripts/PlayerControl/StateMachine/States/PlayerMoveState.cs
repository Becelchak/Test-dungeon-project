using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMoveState : PlayerStateBase
{
    private float speedMove;

    public PlayerMoveState(PlayerStateMachine stateMachine, IPlayerMovementService movementService, Vector3 direction) : base(stateMachine, movementService)
    {}

    public override void Enter()
    {
        base.Enter();
        speedMove = _playerStats.CurrentProfile.speedMove;
        var cameraService = ServiceLocator.Instance.GetService<ICameraService>();
        cameraService?.SetFollowMode();
    }

    public override void Update()
    {
        
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _movementService.SetMovement(_movementService._currentSpeed,
            _playerStats.CurrentProfile.maxSpeed,
            _playerStats.CurrentProfile.acceleration);

        var moveService = (PlayerMovementService)_movementService;
        _stateMachine.charRotate.RotateTowards(moveService.MoveDirection, _playerStats.CurrentProfile.rotationSpeed);
    }

    public override void HandleMoveInput(Vector3 direction)
    {

        _currentInput = Vector3.Lerp(_currentInput, 
            direction, 
            _playerStats.CurrentProfile.acceleration * Time.deltaTime);
        _movementService.UpdateMovementInput(direction);
        _movementService._currentSpeed = Mathf.Lerp(speedMove, 
            _playerStats.CurrentProfile.maxSpeed, 
            _playerStats.CurrentProfile.acceleration * Time.deltaTime);

        if (_inputService._jumpAction.WasPressedThisFrame() && _movementService.CheckGround())
        {
            //var jumpState = new PlayerJumpState(_stateMachine, _movementService, direction);
            //_stateMachine.TransitionToState(jumpState);
        }

        if (direction.magnitude < 0.1f)
        {
            Debug.Log($"{direction.magnitude}");
            var idleState = new PlayerIdleState(_stateMachine, _movementService);
            _stateMachine.TransitionToState(idleState);
        }
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
            _movementService._currentSpeed = Mathf.Lerp(
                speedMove,
                _playerStats.CurrentProfile.maxSpeed,
                _playerStats.CurrentProfile.acceleration * Time.deltaTime
            );
        }
        else
        {
            var idleState = new PlayerIdleState(_stateMachine, _movementService);
            _stateMachine.TransitionToState(idleState);
        }
    }
}
