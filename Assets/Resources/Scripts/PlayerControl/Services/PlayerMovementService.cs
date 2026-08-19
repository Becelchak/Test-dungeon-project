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
    [SerializeField] private float gravity = -9.81f;
    private bool _jumpRequested;

    [Header("ССЫЛКИ")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform hips;

    private Vector3 moveDuraction;
    public CharacterController charController { get; set; }
    private PlayerProfileService playerProfileService;

    private Vector3 _currentInput;
    private Vector2 _rawInput;
    private Vector3 _moveDirection;
    public Vector3 LookDirection { get; private set; }

    private Vector3 _currentVelocity;
    public float _verticalVelocity
    { get; private set; }
    private Vector3 _horizontalMove;

    private bool _isGrounded;
    private bool _isMoving;
    private bool _isRunning;

    public Transform Hips => hips;

    private float _internalSpeed;
    public float _currentSpeed
    {
        get { return _internalSpeed; }
        set { _internalSpeed = value < 0 ? 0 : value; }
    }
    public Vector3 MoveDirection => _moveDirection;

    private IEquipmentStatsService _equipmentStatsService;

    public bool IsRunning { get => _isRunning; set 
        {
            _isRunning = value;
        } 
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        charController = playerObject.GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        var input = ServiceLocator.Instance.GetService<IInputService>();
        input?.EnableGameplayInput();
    }

    public void Start()
    {
        playerProfileService = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _equipmentStatsService = ServiceLocator.Instance.GetService<IEquipmentStatsService>();
        jumpForce = _equipmentStatsService?.CurrentStats?.JumpForce ?? playerProfileService.CurrentProfile.jumpForce;
    }

    public void Jump()
    {
        _jumpRequested = true;
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
        _currentSpeed = 0;
        _horizontalMove = Vector3.zero;
        _verticalVelocity = 0;
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

    public void SetMovement(float targetSpeed, float acceleration)
    {
        if (_moveDirection.magnitude > 0.01f)
        {
            var tempSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            _currentSpeed = tempSpeed;
        }
        else
        {
            _currentSpeed = 0;
        }

        _horizontalMove = _moveDirection * _currentSpeed;
    }

    public void FixedUpdate()
    {
        // Гравитация по умолчанию
        _isGrounded = charController.isGrounded;
        if (_isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f; // небольшое прижатие к земле

        if (_jumpRequested && _isGrounded)
        {
            _verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            _jumpRequested = false;
        }
        _verticalVelocity += gravity * Time.fixedDeltaTime;
        Vector3 finalMove = (_horizontalMove + Vector3.up * _verticalVelocity) * Time.fixedDeltaTime;
        charController.Move(finalMove);
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
