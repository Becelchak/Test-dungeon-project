using UnityEngine;

/// <summary>
/// Структура, описывающая один удар: базовый урон, множители, тип урона,
/// источник, поражённая область и точка попадания.
/// </summary>
public struct DamageInfo
{
    public float BaseDamage;
    public float DamageMultiplier;
    public DamageType DamageType;
    public Transform Attacker;
    public HitboxType HitboxType;
    public Vector3 HitPoint;
    public bool IsCritical;

    /// <summary>Итоговый урон до дополнительных модификаторов брони/баффов.</summary>
    public float FinalDamage => BaseDamage * DamageMultiplier * (IsCritical ? 2f : 1f);
}
