using UnityEngine;

/// <summary>
/// Состояние смерти игрока. Останавливает движение и блокирует ввод до респауна.
/// </summary>
public class PlayerDeadState : PlayerStateBase
{
    public PlayerDeadState(PlayerStateMachine stateMachine, IPlayerMovementService movementService)
        : base(stateMachine, movementService) { }

    public override void Enter()
    {
        base.Enter();
        _movementService.SetMovement(0f, 0f);
        _stateMachine.playerAnimator?.SetBool("IsDead", true);
        Debug.Log("[PlayerDeadState] Игрок мёртв.");
    }

    public override void Update()
    {
        // Удерживаем персонажа на месте
        _movementService.SetMovement(0f, 0f);
    }

    public override void Exit()
    {
        base.Exit();
        _stateMachine.playerAnimator?.SetBool("IsDead", false);
    }

    // Блокируем любой ввод
    public override void HandleMoveInput(Vector3 direction) { }
    public override void HandleAttackInput() { }
    public override void HandleJumpInput(Vector3 direction) { }
    public override void HandleSprintInput(bool inputPressed) { }
    public override void HandleBlockInput(bool isBlocking) { }
    public override void HandleParryInput() { }
    public override void HandleInteractionInput() { }
    public override void HandleMovement(Vector3 direction)
    {
        _movementService.SetMovement(0f, 0f);
    }
}
