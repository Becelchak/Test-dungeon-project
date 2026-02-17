using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementService : BaseService, IPlayerMovementService
{

    [Header("ÍÀÑÒÐÎÉÊÈ ÏÐÛÆÊÀ")]
    // Ïåðåíåñòè â ìîäåëü èãðîêà
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("ÑÑÛËÊÈ")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject playerObject;

    private Vector3 moveDuraction;
    public Rigidbody _rigidbody { get; set; }
    private Transform hips;

    private Vector3 _currentInput;
    private Vector2 _rawInput;
    private Vector3 _moveDirection;
    private Vector3 _currentVelocity;

    private bool _isGrounded;
    private bool _isMoving;

    public float _currentSpeed { get; set; }
    public float CurrentSpeed => _rigidbody.linearVelocity.magnitude;
    public Vector3 MoveDirection => _moveDirection;

    public void Initialize()
    {
        _rigidbody = playerObject.GetComponent<Rigidbody>();
        hips = playerObject.transform.GetChild(2);

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationZ;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        ServiceLocator.Instance.RegisterService<IPlayerMovementService>(this);
    }

    public void Jump(float force)
    {

        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);


        Debug.Log("Èãðîê ïðûãíóë!");
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

    public void SetMovement(float speed, float maxSpeed, float acceleration)
    {
        Vector3 targetVelocity = _moveDirection * Math.Max((_currentSpeed * speed), maxSpeed);
        targetVelocity.y = _rigidbody.linearVelocity.y;

        _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }
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

    //public void ModifySpeed(float multiplier, float duration)
    //{
    //    moveSpeed *= multiplier;
    //    maxSpeed *= multiplier;
    //    Invoke(nameof(ResetSpeed), duration);
    //}

    //private void ResetSpeed()
    //{
    //    moveSpeed = 8f;
    //    maxSpeed = 12f;
    //}

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
