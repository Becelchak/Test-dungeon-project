using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Источник урона оружия. Вешается на коллайдер клинка/дубины/щита.
/// Активируется только в нужные кадры атаки, чтобы не наносить урон постоянно.
/// </summary>
public class WeaponDamageSource : MonoBehaviour
{
    [Tooltip("Активен ли сейчас этот источник урона")]
    public bool IsActive;

    [Tooltip("Базовый урон за попадание")]
    public float BaseDamage;

    [Tooltip("Тип урона")]
    public DamageType DamageType = DamageType.True;

    [Tooltip("Владелец оружия. Если не назначен — ищется автоматически: ближайший IDamageable среди родителей (NpcController для NPC, PlayerDamageReceiver для игрока).")]
    [SerializeField] private Transform _ownerOverride;

    private Transform _owner;
    private bool _ownerResolved;
    private HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    /// <summary>
    /// Определяет владельца оружия: явный override, иначе ближайший IDamageable
    /// среди родителей, иначе корень иерархии (старое поведение).
    /// </summary>
    private Transform ResolveOwner()
    {
        if (_ownerOverride != null) return _ownerOverride;
        if (!_ownerResolved)
        {
            var damageable = GetComponentInParent<IDamageable>();
            _owner = damageable != null ? damageable.Transform : null;
            _ownerResolved = true;
        }
        return _owner;
    }

    private void Awake()
    {
        // Для срабатывания OnTriggerEnter хотя бы на одном из объектов должен быть Rigidbody
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning($"[WeaponDamageSource] Коллайдер на {gameObject.name} не является триггером. Урон не будет регистрироваться. Включи IsTrigger.");
        }
    }

    /// <summary>
    /// Сбрасывает список уже поражённых целей. Вызывается при начале новой атаки.
    /// </summary>
    public void ResetHits() => _hitTargets.Clear();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive) return;

        // Проверка попадания в щит
        var shieldMarker = other.GetComponent<ShieldMarker>();
        if (shieldMarker != null)
        {
            // Проверка блокирует ли владелец щита или просто носит его
            if (IsShieldOwnerBlocking(shieldMarker.Owner))
            {
                Debug.Log($"[WeaponDamageSource] Атака заблокирована щитом {other.gameObject.name}");
            }
        }

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        Transform owner = ResolveOwner();

        // Не бьём самих себя
        if (owner != null)
        {
            if (target.Transform == owner
                || target.Transform.IsChildOf(owner)
                || owner.IsChildOf(target.Transform)) return;
        }
        else if (target.Transform.root == transform.root) return;

        // Один взмах — одно попадание в цель
        if (_hitTargets.Contains(target)) return;

        var hitbox = other.GetComponent<NpcHitbox>();
        float multiplier = hitbox != null ? hitbox.DamageMultiplier : 1f;
        if(hitbox != null)
            Debug.Log($"Удар пришелся по {hitbox.HitboxType} {target.Transform.name}!");

        var info = new DamageInfo
        {
            BaseDamage = BaseDamage,
            DamageMultiplier = multiplier,
            DamageType = DamageType,
            Attacker = owner != null ? owner : transform.root,
            HitboxType = hitbox != null ? hitbox.HitboxType : HitboxType.Other,
            HitPoint = other.ClosestPoint(transform.position),
            WasBlocked = shieldMarker != null && IsShieldOwnerBlocking(shieldMarker.Owner),
            ShieldHit = shieldMarker != null ? other.gameObject : null
        };

        target.ApplyDamage(info);
        _hitTargets.Add(target);
    }

    /// <summary>
    /// Проверяет, блокирует ли владелец щита в данный момент.
    /// </summary>
    private bool IsShieldOwnerBlocking(Transform shieldOwner)
    {
        if (shieldOwner == null) return false;

        // Ищем боевой сервис игрока
        var combatService = shieldOwner.GetComponent<IPlayerCombatService>();
        if (combatService == null)
        {
            // Пробуем найти через ServiceLocator
            combatService = ServiceLocator.Instance?.GetService<IPlayerCombatService>();
        }

        return combatService != null && combatService.IsBlocking;
    }
}
