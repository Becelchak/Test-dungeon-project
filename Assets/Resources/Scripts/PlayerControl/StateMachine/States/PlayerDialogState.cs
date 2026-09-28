
using UnityEngine;

public class PlayerDialogState : PlayerStateBase
{
    public PlayerDialogState(PlayerStateMachine stateMachine, IPlayerMovementService movementService) : base(stateMachine, movementService)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _inputService.DisableAllInput();
        Debug.Log("Вошли в состояние диалога - ввод отключен");
    }

    public override void Exit()
    {
        base.Exit();
        _inputService.EnableAllInput();
        Debug.Log("Вышли из состояния диалога — ввод включён");
    }
}
