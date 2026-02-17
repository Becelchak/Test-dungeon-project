using EventBusSystem;
using System;
using UnityEngine;

/// <summary>
/// Базовый абстрактный класс для всех состояний игрока
/// </summary>
public abstract class PlayerStateBase
{
    protected PlayerStateMachine _stateMachine;
    protected IPlayerMovementService _movementService;
    protected PlayerProfileService _playerStats;
    protected IInputService _inputService;
    protected Vector3 _currentInput;

    protected float _timeEnteredState;

    public PlayerStateBase(PlayerStateMachine stateMachine, IPlayerMovementService movementService)
    {
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));

        _inputService = ServiceLocator.Instance.GetService<IInputService>();
        _movementService = (PlayerMovementService) ServiceLocator.Instance.GetService<IPlayerMovementService>();
        _playerStats = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
    }

    public virtual void Enter()
    {
        _timeEnteredState = Time.time;
        Debug.Log($"Вошли в состояние: {GetType().Name}");

        EventBus.RaiseEvent<IPlayerStateSubscriber>(s => s.OnPlayerStateChanged(GetType().Name));
    }
    public virtual void Exit()
    {
        Debug.Log($"Вышли из состояния: {GetType().Name}");
    }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void HandleMoveInput(Vector3 direction) { }
    public virtual void HandleAttackInput() { }
    public virtual void HandleInteractInput() { }
    public virtual void HandleDashInput() { }
    public virtual void HandleSprintInput(bool inputPressed) { }
    public virtual void HandleMovement(Vector3 direction) { }
}