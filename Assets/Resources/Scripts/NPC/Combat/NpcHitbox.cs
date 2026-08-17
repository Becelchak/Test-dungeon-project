using UnityEngine;

/// <summary>
/// Хитбокс части тела NPC. Вешается на коллайдеры костей (Head, Spine и т.д.).
/// Содержит множитель урона и тип поражённой зоны.
/// </summary>
public class NpcHitbox : MonoBehaviour
{
    [Tooltip("Тип поражённой части тела")]
    [SerializeField] private HitboxType _hitboxType = HitboxType.Other;

    [Tooltip("Множитель урона для этой части тела")]
    [SerializeField] private float _damageMultiplier = 1f;

    public HitboxType HitboxType => _hitboxType;
    public float DamageMultiplier => _damageMultiplier;
}
