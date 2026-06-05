using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementService : BaseService, IPlayerMovementService
{

    [Header("НАСТРОЙКИ ПРЫЖКА")]
    // Перенести в модель игрока
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("ССЫЛКИ")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform hips;

    private Vector3 moveDuraction;
    public CharacterController charController { get; set; }

    private Vector3 _currentInput;
    private Vector2 _rawInput;
    private Vector3 _moveDirection;
    public Vector3 LookDirection { get; private set; }

    private Vector3 _currentVelocity;
    private float _verticalVelocity;

    private bool _isGrounded;
    private bool _isMoving;

    public Transform Hips => hips;

    private float _internalSpeed;
    public float _currentSpeed
    {
        get { return _internalSpeed; }
        set { _internalSpeed = value < 0 ? 0 : value; }
    }
    public Vector3 MoveDirection => _moveDirection;

    protected override void Awake()
    {
        Debug.Log("PlayerMovementService проснулся");
        base.Awake();
    }

    public void Initialize()
    {
        charController = playerObject.GetComponent<CharacterController>();

        //charController.interpolation = RigidbodyInterpolation.Interpolate;
        //charController.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //charController.constraints = RigidbodyConstraints.FreezeRotationX |
        //                         RigidbodyConstraints.FreezeRotationZ;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        var input = ServiceLocator.Instance.GetService<IInputService>();
        input?.EnableGameplayInput();
    }

    public void Jump(float force, Vector3 direction)
    {
        charController.Move(Vector3.up * force);
    }

    public void StartRun()
    {
    }

    public void StopRun()
    {

    }

    public void StopMovement()
    {
        moveDuraction = Vector3.zero;
    }

    public void UpdateLookDirection(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
            LookDirection = direction.normalized;
    }

    public Vector2 GetLocalMovementInput(Transform reference)
    {
        Vector3 worldDir = _moveDirection;
        Vector3 localDir = reference.InverseTransformDirection(worldDir);
        return new Vector2(localDir.x, localDir.z);
    }

    public void SetMovement(float speed, float maxSpeed, float acceleration)
    {
        Vector3 targetVelocity = _moveDirection * Math.Max((_currentSpeed * speed), maxSpeed);
        if (_moveDirection == Vector3.zero)
        {
            _currentSpeed = 0;
        }
        else
        {
            float targetSpd = Mathf.Clamp(_currentSpeed * speed, 0f, maxSpeed);
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpd, acceleration * Time.fixedDeltaTime);
        }
        float verticalVelocity = _verticalVelocity;
        if (CheckGround() && verticalVelocity < 0)
            verticalVelocity = -2f; // Небольшое прижатие к земле, чтобы персонаж не "летал"

        // Применяем гравитацию
        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        // Формируем итоговое перемещение (горизонтальное движение + гравитация)
        Vector3 move = _moveDirection * _currentSpeed * Time.deltaTime;
        move.y = verticalVelocity;

        charController.Move(move);
    }

    protected override Type GetServiceType() => typeof(IPlayerMovementService);

    public void UpdateMovementInput(Vector2 input)
    {
        _rawInput = input;
        CalculateMovementDirection();
    }

    public void CalculateMovementDirection()
    {
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        _moveDirection = (cameraForward * _rawInput.y + cameraRight * _rawInput.x).normalized;
    }

    public bool CheckGround()
    {
        RaycastHit hit;
        _isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance,
            _isGrounded ? Color.green : Color.red);

        return _isGrounded;
    }

    public bool IsMoving()
    {
        return _currentInput.magnitude > 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.2f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _moveDirection * 2f);
        }
    }
}
