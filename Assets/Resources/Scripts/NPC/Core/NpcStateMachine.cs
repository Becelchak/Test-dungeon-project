using EventBusSystem;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State Machine NPC. Хранит ссылки на все сервисы и управляет текущим состоянием.
/// </summary>
public class NpcStateMachine : MonoBehaviour, IParryEventSubscriber
{
    public NpcController Controller { get; private set; }
    public Animator Animator { get; private set; }
    public NpcAnimationController AnimationController { get; private set; }
    public NpcCombatService Combat { get; private set; }
    public NpcPerception Perception { get; private set; }
    public NpcHealthService Health { get; private set; }
    public NpcDefenseService Defense { get; private set; }
    public NpcTacticsService Tactics { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    private NpcBaseState _currentState;

    public void Initialize(NpcController controller)
    {
        Controller = controller;
        Animator = GetComponentInChildren<Animator>();
        AnimationController = GetComponent<NpcAnimationController>();
        Combat = GetComponent<NpcCombatService>();
        Perception = GetComponent<NpcPerception>();
        Health = GetComponent<NpcHealthService>();
        Defense = GetComponent<NpcDefenseService>();
        Tactics = GetComponent<NpcTacticsService>();
        Agent = GetComponent<NavMeshAgent>();

        Defense?.Initialize(controller.Data);
        Tactics?.Initialize(controller.Data);

        TransitionToState(new NpcIdleState(this));
        EventBus.Subscribe(this);
    }

    private void Update()
    {
        _currentState?.Update();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    public void TransitionToState(NpcBaseState newState)
    {
        if (!enabled) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public void OnParryEvent(ParrySuccessEvent parryEvent)
    {
        if (Perception.CurrentTarget == null)
            return;
        if (parryEvent.sourceParry == Perception.CurrentTarget.gameObject)
        {
            TransitionToState(new NpcStaggerState(this));
            Debug.Log($"Успешно застанили {parryEvent.targetParry}!");
        }
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe(this);
    }
}
