using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

public class PlayerJumpState : PlayerStateBase
{
    private bool _hasJumped; // флаг, чтобы не прыгать повторно
    private bool _hasStartedFall = false;
    private bool _isLandingHandled = false;
    private Vector3 jumpDirection;

    public PlayerJumpState(PlayerStateMachine stateMachine, IPlayerMovementService movementService, Vector3 direction)
        : base(stateMachine, movementService) { jumpDirection = direction; }

    public override void Enter()
    {
        base.Enter();
        _movementService.Jump();
        _stateMachine.playerAnimator.SetTrigger("JumpStart");
        _hasJumped = true;
        _hasStartedFall = false;
        _isLandingHandled = false;
        _movementService.IsRunning = false;

    }

    public override void Update()
    {
        bool isGrounded = _movementService.CheckGround();
        Debug.Log($"grounded = {isGrounded}");
        bool isFalling = _movementService._verticalVelocity < -0.5f;

        _stateMachine.playerAnimator.SetBool("InAir", !isGrounded);
        _stateMachine.playerAnimator.SetBool("IsGrounded", isGrounded);

        // Переход в фазу падения после пика прыжка
        if (_hasJumped && !_hasStartedFall && isFalling)
        {
            _hasStartedFall = true;
            _stateMachine.playerAnimator.SetTrigger("JumpFall");
        }

        // Переход в фазу приземления после прыжка
        if (_hasJumped && isGrounded && _movementService._verticalVelocity <= 0 && !_isLandingHandled)
        {
            _isLandingHandled = true; // Блокируем повторный вход

            // Запускаем асинное ожидание окончания анимации
            HandleLandingAndExit().Forget();
        }

        if(!_hasJumped)
            CheckExitToMovement();
    }

    private void CheckExitToMovement()
    {
        if (_movementService.CheckGround())
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

    private async UniTaskVoid HandleLandingAndExit()
    {
        
        await UniTask.Yield();

        // Ожидание, пока анимация JumpLand проиграется до конца
        // 0 — индекс слоя анимации (Base Layer - для всего тела)
        while (_stateMachine.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            // Уступаем ход Unity на один кадр
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        _hasJumped = false;
        _stateMachine.playerAnimator.SetTrigger("JumpLand");
        _stateMachine.playerAnimator.ResetTrigger("JumpFall");
        // Анимация полностью завершилась — проверяем можно ли перейти в иное состояние
        CheckExitToMovement();
    }

    public override void HandleMoveInput(Vector3 direction)
    {
        _movementService.UpdateMovementInput(direction);
    }

    public override void Exit()
    {
        _hasJumped = false;
        _hasStartedFall = false;
        _isLandingHandled = false;

        var isGrounded = _movementService.CheckGround();
        _stateMachine.playerAnimator.SetBool("InAir", !isGrounded);
        _stateMachine.playerAnimator.SetBool("IsGrounded", isGrounded);
        _stateMachine.playerAnimator.ResetTrigger("JumpStart");

        Debug.Log($"grounded exit = {isGrounded}");
        base.Exit();
    }

}
