
using UnityEngine;

public class PlayerDialogState : PlayerStateBase
{
    public PlayerDialogState(PlayerStateMachine stateMachine, IPlayerMovementService movementService) : base(stateMachine, movementService)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _inputService.DisableGameplayInput();
        Debug.Log("Вошли в состояние диалога - ввод отключен");
    }

    public override void Exit()
    {
        base.Exit();
        _inputService.EnableGameplayInput();
        Debug.Log("Вышли из состояния диалога — ввод включён");
    }
}
