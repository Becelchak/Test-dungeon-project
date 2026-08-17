using UnityEngine;

/// <summary>
/// Верхний уровень управления NPC. Реализует IDamageable и связывает сервисы между собой.
/// </summary>
public class NpcController : MonoBehaviour, IDamageable
{
    [Tooltip("Данные поведения и характеристик NPC")]
    [SerializeField] private NpcData _data;

    public NpcData Data => _data;
    public Transform Transform => transform;
    public bool IsAlive { get; private set; } = true;

    private NpcStateMachine _stateMachine;
    private NpcHealthService _health;
    private NpcCombatService _combat;
    private NpcPerception _perception;
    private NpcAnimationController _animation;

    private void Awake()
    {
        _stateMachine = GetComponent<NpcStateMachine>();
        _health = GetComponent<NpcHealthService>();
        _combat = GetComponent<NpcCombatService>();
        _perception = GetComponent<NpcPerception>();
        _animation = GetComponent<NpcAnimationController>();
    }

    private void Start()
    {
        if (_data == null)
        {
            Debug.LogError($"[NpcController] На {gameObject.name} не назначен NpcData!");
            enabled = false;
            return;
        }

        _health.Initialize(_data.maxHealth);
        _combat.Initialize(_data);
        _stateMachine.Initialize(this);

        _health.OnDamaged += OnDamaged;
        _health.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= OnDamaged;
            _health.OnDeath -= OnDeath;
        }
    }

    public void ApplyDamage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;
        _health.ApplyDamage(damageInfo);
    }

    private void OnDamaged()
    {
        _animation?.TriggerHit();
    }

    private void OnDeath()
    {
        if (!IsAlive) return;
        IsAlive = false;

        _animation?.TriggerDeath();
        _stateMachine.enabled = false;
        _combat.enabled = false;
        _perception.enabled = false;

        // TODO: включить ragdoll, отключить коллайдеры, дроп лута и т.д.
        Debug.Log($"[NpcController] {gameObject.name} погиб.");
    }
}
