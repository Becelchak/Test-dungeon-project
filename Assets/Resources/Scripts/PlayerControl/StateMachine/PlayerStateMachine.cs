using EventBusSystem;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour, IDialogueEventSubscriber
{
    private IInputService _input;
    private IPlayerMovementService _movement;
    private PlayerStateBase _currentState;
    public CharacterRotator charRotate;
    public Interactor interactor;
    public Animator playerAnimator;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    private void Awake()
    {
        _input = ServiceLocator.Instance.GetService<IInputService>();
        _movement = ServiceLocator.Instance.GetService<IPlayerMovementService>();
        _movement.Initialize();
        charRotate = GetComponent<CharacterRotator>();
        //interactor = GameObject.Find("Interactor").GetComponent<Interactor>();
        interactor = GetComponentInChildren<Interactor>();
        playerAnimator = GetComponentInChildren<Animator>();
    }
    private void Start()
    {
        _input.OnMove += HandleMoveInput;
        _input.OnAttack += HandleAttackInput;
        _input.OnSprintInput += HandleSprintInput;
        _input.OnRun += HandleRunInput;
        _input.OnInteract += HandleInteractionInput;
        _input.OnJump += HandleJumpInput;

        TransitionToState(new PlayerIdleState(this, _movement));
    }


    private void HandleJumpInput()
    {
        var direction = _input.GetMovementInput();
        _currentState?.HandleJumpInput(direction);
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

    private void HandleInteractionInput()
    {
        _currentState?.HandleInteractionInput();
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

    public void OnDialogueStarted(string npcId, DialogueType dialogueType)
    {
        if (_currentState is PlayerDialogState) return;
        TransitionToState(new PlayerDialogState(this, _movement));
    }

    public void OnDialogueEnded()
    {
        TransitionToState(new PlayerIdleState(this, _movement));
    }

    public void OnResponseSelected(string responseId)
    {
        
    }

    public void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnMove -= HandleMoveInput;
            _input.OnAttack -= HandleAttackInput;
            _input.OnSprintInput -= HandleSprintInput;
            _input.OnRun -= HandleRunInput;
            _input.OnInteract -= HandleInteractionInput;
            _input.OnJump -= HandleJumpInput;
        }
    }
}