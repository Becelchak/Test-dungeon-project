using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    private bool _hasJumped; // флаг, чтобы не прыгать повторно
    private Vector3 jumpDirection;

    public PlayerJumpState(PlayerStateMachine stateMachine, IPlayerMovementService movementService, Vector3 direction)
        : base(stateMachine, movementService) { jumpDirection = direction; }

    public override void Enter()
    {
        base.Enter();
        _movementService.Jump(_playerStats.CurrentProfile.jumpForce, jumpDirection);
        _hasJumped = true;

        // Ќе отключаем ввод Ц игрок может управл€ть в воздухе
        // _inputService.DisableGameplayInput(); // ”ƒјЋ»“№
    }

    public override void Update()
    {
        if (_hasJumped && _movementService.CheckGround())
        {
            Vector2 input = _inputService.GetMovementInput();
            if (input.magnitude > 0.1f)
            {
                var moveState = new PlayerMoveState(_stateMachine, _movementService, input);
                _stateMachine.TransitionToState(moveState);
            }
            else
            {
                var idleState = new PlayerIdleState(_stateMachine, _movementService);
                _stateMachine.TransitionToState(idleState);
            }
        }
    }

    public override void HandleMoveInput(Vector3 direction)
    {
        _movementService.UpdateMovementInput(direction);
    }

    public override void Exit()
    {
        base.Exit();
        // _inputService.EnableGameplayInput(); // ”ƒјЋ»“№
    }

}
