using UnityEngine;

/// <summary>
/// Общий контракт для всех сущностей, которые могут получать урон.
/// </summary>
public interface IDamageable
{
    /// <summary>Корневой трансформ сущности.</summary>
    Transform Transform { get; }

    /// <summary>
    /// Применить урон к сущности.
    /// </summary>
    void ApplyDamage(DamageInfo damageInfo);
}
