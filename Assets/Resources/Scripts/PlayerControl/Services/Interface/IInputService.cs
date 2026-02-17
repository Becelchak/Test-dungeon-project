using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputService
{
    event Action<Vector2> OnMove;
    event Action OnRun;
    event Action OnAttack;
    event Action OnInteract;
    event Action OnJump;
    event Action<bool> OnSprintInput;

    InputAction _moveAction { get; set; }
    InputAction _jumpAction { get; set; }
    InputAction _attackAction { get; set; }
    InputAction _interactAction { get; set; }

    Vector2 GetMovementInput();
    void EnableGameplayInput();
    void DisableGameplayInput();
}