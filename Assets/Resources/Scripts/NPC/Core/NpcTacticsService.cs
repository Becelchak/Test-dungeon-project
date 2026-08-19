using EventBusSystem;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Сервис тактики NPC. Отслеживает состояние игрока (атака, здоровье) и предлагает
/// тактические реакции: блок, обход, отступление, агрессивное наступление.
/// </summary>
public class NpcTacticsService : MonoBehaviour, IPlayerStateSubscriber, IHealthChangedEventSubscriber
{
    [Tooltip("Время в секундах, в течение которого NPC считает, что игрок всё ещё атакует, после события атаки")]
    [SerializeField] private float _playerAttackMemory = 0.8f;

    public bool IsPlayerAttacking { get; private set; }
    public float PlayerHealthPercent { get; private set; } = 1f;
    public float NpcHealthPercent => _health != null ? (float)_health.CurrentHealth / Mathf.Max(1, _health.MaxHealth) : 1f;

    private float _lastPlayerAttackTime = -999f;
    private NpcData _data;
    private NpcHealthService _health;
    private NpcPerception _perception;

    public void Initialize(NpcData data)
    {
        _data = data;
        _health = GetComponent<NpcHealthService>();
        _perception = GetComponent<NpcPerception>();

        EventBus.Subscribe(this);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(this);
    }

    private void Update()
    {
        if (IsPlayerAttacking && Time.time - _lastPlayerAttackTime > _playerAttackMemory)
            IsPlayerAttacking = false;
    }

    public void OnPlayerStateChanged(string stateName)
    {
        if (stateName == nameof(PlayerAttackState))
        {
            IsPlayerAttacking = true;
            _lastPlayerAttackTime = Time.time;
        }
    }

    public void OnHealthChanged(HealthChangedEvent evt)
    {
        if (evt.MaxHealth > 0)
            PlayerHealthPercent = (float)evt.CurrentHealth / evt.MaxHealth;
    }

    /// <summary>
    /// Оценивает ситуацию и возвращает предложенное тактическое состояние,
    /// либо null, если стандартное поведение должно продолжаться.
    /// </summary>
    public Type EvaluateTacticalState()
    {
        if (_data == null || !_perception.HasTarget)
            return null;

        // 1. Отступление при низком здоровье NPC
        if (NpcHealthPercent <= _data.retreatHealthThreshold && Random.value <= _data.retreatChance)
            return typeof(NpcRetreatState);

        // 2. Блок, если игрок атакует и NPC находится в ближней дистанции
        if (IsPlayerAttacking && _perception.IsTargetInAttackRange && Random.value <= _data.blockChance)
            return typeof(NpcBlockState);

        // 3. Обход, если игрок атакует, но блок не сработал
        if (IsPlayerAttacking && Random.value <= _data.strafeChance)
            return typeof(NpcStrafeState);

        // 4. Агрессивное наступление, если у игрока мало здоровья
        if (PlayerHealthPercent <= _data.aggressivePlayerHealthThreshold && Random.value <= _data.aggressiveChance)
            return typeof(NpcAggressiveChaseState);

        return null;
    }
}
