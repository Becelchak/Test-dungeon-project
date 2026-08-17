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
    public DamageType DamageType = DamageType.Physical;

    private HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

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

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        // Не бьём самих себя и свою команду
        if (target.Transform.root == transform.root) return;

        // Один взмах — одно попадание в цель
        if (_hitTargets.Contains(target)) return;

        var hitbox = other.GetComponent<NpcHitbox>();
        float multiplier = hitbox != null ? hitbox.DamageMultiplier : 1f;

        var info = new DamageInfo
        {
            BaseDamage = BaseDamage,
            DamageMultiplier = multiplier,
            DamageType = DamageType,
            Attacker = transform.root,
            HitboxType = hitbox != null ? hitbox.HitboxType : HitboxType.Other,
            HitPoint = other.ClosestPoint(transform.position)
        };

        target.ApplyDamage(info);
        _hitTargets.Add(target);
    }
}
