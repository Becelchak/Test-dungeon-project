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
    event Action OnSubmit;
    event Action<bool> OnSprintInput;

    InputAction _moveAction { get; set; }
    InputAction _jumpAction { get; set; }
    InputAction _attackAction { get; set; }
    InputAction _interactAction { get; set; }
    InputAction _submitAction { get; set; }

    Vector2 GetMovementInput();
    void EnableGameplayInput();
    void DisableGameplayInput();
    Vector3 GetMouseWorldDirection(Camera cam, Transform playerTransform, float planeY = 0f);
}