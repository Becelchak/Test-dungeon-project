using EventBusSystem;
using System.Collections;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour, IDialogueEventSubscriber, IPlayerDiedEventSubscriber
{
    private IInputService _input;
    private IPlayerMovementService _movement;
    private IEquipmentService _equipment;
    private IPlayerCombatService _combat;

    public IPlayerCombatService CombatService => _combat;
    private PlayerStateBase _currentState;
    private Coroutine _parryResetCoroutine;
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
        _input.OnParry += HandleParryInput;
        _input.OnDodge += HandleDodgeInput;

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

    private void HandleDodgeInput()
    {
        var direction = _input.GetMovementInput();
        _currentState?.HandleDodgeInput(direction);
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
        if (_combat == null)
            _combat = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        _currentState?.HandleAttackInput();
    }

    private void HandleBlockInput(bool isBlocking)
    {
        if (_combat == null)
            _combat = ServiceLocator.Instance.GetService<IPlayerCombatService>();

        _combat?.SetBlocking(isBlocking);
        weaponIKController.SetPose(
            isBlocking ? WeaponIKController.WeaponPose.Block : WeaponIKController.WeaponPose.Idle,
            true);
        _currentState?.HandleBlockInput(isBlocking);
        playerAnimationController?.SetBlocking(isBlocking);
    }

    private void HandleParryInput()
    {
        if (_combat == null)
            _combat = ServiceLocator.Instance.GetService<IPlayerCombatService>();

        if (_combat == null || !_combat.TryStartParry())
            return;

        weaponIKController.SetPose(WeaponIKController.WeaponPose.Parry, false);
        _currentState?.HandleParryInput();
        playerAnimationController?.TriggerParry();

        float waitTime = playerAnimationController.GetCurrentParryClipLength();
        if (_parryResetCoroutine != null)
            StopCoroutine(_parryResetCoroutine);
        _parryResetCoroutine = StartCoroutine(ResetParryPoseAfter(waitTime));
    }

    private IEnumerator ResetParryPoseAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _parryResetCoroutine = null;

        weaponIKController.SetPose(
            _combat != null && _combat.IsBlocking
                ? WeaponIKController.WeaponPose.Block
                : WeaponIKController.WeaponPose.Idle,
            true);
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

    public void OnPlayerDied(PlayerDiedEvent evt)
    {
        if (_currentState is PlayerDeadState) return;
        TransitionToState(new PlayerDeadState(this, _movement));
        _movement.StopMovement();
        playerAnimationController.EquipmentService.SetPlayerStatus(true);
    }

    public void RevivePlayer()
    {
        _combat?.Revive();
        if (!(_currentState is PlayerIdleState))
            TransitionToState(new PlayerIdleState(this, _movement));
        playerAnimator.SetBool("IsAlive", true);
        playerAnimationController.EquipmentService.SetPlayerStatus(false);
        charRotate.OnRevivePlayer();
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
            _input.OnParry -= HandleParryInput;
            _input.OnDodge -= HandleDodgeInput;
        }
    }
}