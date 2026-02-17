using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private IInputService _input;
    private IPlayerMovementService _movement;
    private PlayerStateBase _currentState;
    public CharacterRotator charRotate;

    private void Start()
    {
        _input = ServiceLocator.Instance.GetService<IInputService>();
        _movement = ServiceLocator.Instance.GetService<IPlayerMovementService>();
        _movement.Initialize();
        charRotate = GetComponent<CharacterRotator>();

        _input.OnMove += HandleMoveInput;
        _input.OnAttack += HandleAttackInput;
        _input.OnSprintInput += HandleSprintInput;
        _input.OnRun += HandleRunInput;

        TransitionToState(new PlayerIdleState(this, _movement));
    }

    private void HandleMoveInput(Vector2 direction)
    {
        // Только текущее состояние решает, как обработать ввод
        _currentState?.HandleMoveInput(direction);
    }

    private void HandleSprintInput(bool sprintInpitPressed)
    {
        _currentState?.HandleSprintInput(sprintInpitPressed);
    }
    private void Update()
    {
        Vector2 currentInput = _input.GetMovementInput();
        _currentState?.HandleMovement(currentInput);
        _currentState?.Update();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }
    private void HandleAttackInput()
    {
        _currentState?.HandleAttackInput();
    }

    private void HandleRunInput()
    {
        _currentState?.HandleAttackInput();
    }

    public void TransitionToState(PlayerStateBase newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void OnMoveInput(Vector2 direction) => _currentState?.HandleMoveInput(direction);
    public void OnAttackInput() => _currentState?.HandleAttackInput();
}