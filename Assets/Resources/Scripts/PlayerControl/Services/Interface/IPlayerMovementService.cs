using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IPlayerMovementService
{

    CharacterController charController {  get; set; }
    float _currentSpeed  {  get; set; }
    void Initialize();
    void Jump(float force, Vector3 direction);
    bool CheckGround();
    void StartRun();
    void StopRun();
    bool IsMoving();
    void SetMovement(float speed, float maxSpeed, float acceleration);
    void StopMovement();
    void CalculateMovementDirection();
    void UpdateMovementInput(Vector2 input);
}
