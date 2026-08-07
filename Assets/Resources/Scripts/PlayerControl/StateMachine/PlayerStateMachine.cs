using EventBusSystem;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour, IDialogueEventSubscriber
{
    private IInputService _input;
    private IPlayerMovementService _movement;
    private IEquipmentService _equipment;
    private IPlayerCombatService _combat;

    public IPlayerCombatService CombatService => _combat;
    private PlayerStateBase _currentState;
    public CharacterRotator charRotate;
    public Interactor interactor;
    public Animator playerAnimator;
    public PlayerAnimationController playerAnimationController;
    public WeaponIKController weaponIKController;

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
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _combat = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        if (_combat == null)
            Debug.LogError("[PlayerStateMachine] PlayerCombatService не найден в ServiceLocator! Добавьте компонент PlayerCombatService на сцену.");
        charRotate = GetComponent<CharacterRotator>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        //interactor = GameObject.Find("Interactor").GetComponent<Interactor>();
        interactor = GetComponentInChildren<Interactor>();
        playerAnimator = GetComponentInChildren<Animator>();
        weaponIKController = GetComponentInChildren<WeaponIKController>();
    }
    private void Start()
    {
        _input.OnMove += HandleMoveInput;
        _input.OnAttack += HandleAttackInput;
        _input.OnSprint += HandleSprintInput;
        _input.OnInteract += HandleInteractionInput;
        _input.OnJump += HandleJumpInput;
        _input.OnBlock += HandleBlockInput;

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

    private void HandleBlockInput(bool isBlocking)
    {
        if (_combat == null)
            _combat = ServiceLocator.Instance.GetService<IPlayerCombatService>();

        _combat?.SetBlocking(isBlocking);
        _currentState?.HandleBlockInput(isBlocking);
        playerAnimationController?.SetBlocking(isBlocking);
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
            _input.OnSprint -= HandleSprintInput;
            _input.OnInteract -= HandleInteractionInput;
            _input.OnJump -= HandleJumpInput;
            _input.OnBlock -= HandleBlockInput;
        }
    }
}