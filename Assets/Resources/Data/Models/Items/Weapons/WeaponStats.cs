using System;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    [Tooltip("Уникальный ID для связи")]
    public string weaponId;
    public float damage = 10f;
    public float range = 2f;
    public float attackDuration = 0.5f;

    [Tooltip("Окно идеального блока/парирования в секундах после нажатия кнопки блока. Зависит от оружия.")]
    public float perfectBlockWindow = 0.5f;

    [Tooltip("Поглощение урона обычным блоком (0 = блок не поглощает, 1 = поглощает весь урон).")]
    [Range(0f, 1f)]
    public float blockDamageReduction = 0.5f;
}