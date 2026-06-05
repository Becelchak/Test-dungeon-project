using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerService : BaseService, IInputService
{
    private InputActionAsset _inputActions;
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnRun;
    public event Action OnSubmit;
    public event Action<bool> OnSprintInput;
    
    public InputAction _moveAction { get; set; }
    public InputAction _jumpAction { get; set; }
    public InputAction _attackAction { get; set; }
    public InputAction _interactAction { get; set; }
    public InputAction _submitAction { get; set; }

    protected override Type GetServiceType() => typeof(IInputService);


    private void Awake()
    {
        base.Awake();
        _inputActions = InputSystem.actions;
        SetupCallbacks();
        EnableGameplayInput();
    }

    protected void Start()
    {
        Debug.Log("Start input");
    }

    private void SetupCallbacks()
    {
        _moveAction = _inputActions.FindAction("Move");
        _moveAction.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _moveAction.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        _jumpAction = _inputActions.FindAction("Jump");
        _jumpAction.performed += ctx => OnJump?.Invoke();
        _jumpAction.canceled += ctx => OnJump?.Invoke();

        _attackAction = _inputActions.FindAction("Attack");
        _attackAction.performed += ctx => OnAttack?.Invoke();
        _attackAction.canceled += ctx => OnAttack?.Invoke();

        _interactAction = _inputActions.FindAction("Interact");
        _interactAction.performed += ctx => OnInteract?.Invoke();
        //_interactAction.canceled += ctx => OnInteract?.Invoke();

        _submitAction = _inputActions.FindAction("Submit");
        _submitAction.performed += ctx => OnSubmit?.Invoke();
    }

    public Vector2 GetMovementInput()
    {
        if (_moveAction == null) return Vector2.zero;
        return _moveAction.ReadValue<Vector2>();
    }

    public Vector3 GetMouseWorldDirection(Camera cam, Transform playerTransfrom, float planeY = 0f)
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 fromPlayer = point - playerTransfrom.position;
            fromPlayer.y = 0;
            return fromPlayer.normalized;
        }
        return playerTransfrom.forward;
    }

    public void EnableGameplayInput() => _inputActions.Enable();
    public void DisableGameplayInput()
    {
        _inputActions.Disable();
        Debug.Log("Off input");
    }
}
