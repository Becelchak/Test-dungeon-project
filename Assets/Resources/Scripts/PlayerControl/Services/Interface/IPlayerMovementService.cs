using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IPlayerMovementService
{

    CharacterController charController { get; set; }
    float _currentSpeed  {  get; set; }
    float _verticalVelocity { get; }
    void Initialize();
    void Jump();
    bool CheckGround();
    void StartRun();
    void StopRun();
    bool IsMoving();
    void SetMovement(float speed, float maxSpeed, float acceleration);
    void StopMovement();
    void CalculateMovementDirection();
    void UpdateMovementInput(Vector2 input);
}
