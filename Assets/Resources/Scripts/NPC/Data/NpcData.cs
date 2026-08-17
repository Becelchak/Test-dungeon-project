using UnityEngine;

/// <summary>
/// Данные типа NPC. Создаётся как ScriptableObject для каждого вида врага.
/// </summary>
[CreateAssetMenu(fileName = "NpcData", menuName = "Data/NPC/Npc Data")]
public class NpcData : ScriptableObject
{
    [Header("Здоровье")]
    [Tooltip("Максимальное здоровье NPC")]
    public int maxHealth = 100;

    [Header("Передвижение")]
    [Tooltip("Базовая скорость передвижения")]
    public float moveSpeed = 3.5f;

    [Tooltip("Скорость поворота к цели")]
    public float rotationSpeed = 5f;

    [Header("Обнаружение")]
    [Tooltip("Радиус, в котором NPC может обнаружить игрока")]
    public float detectionRadius = 10f;

    [Tooltip("Угол обзора относительно направления взгляда (0-360)")]
    [Range(0f, 360f)]
    public float detectionAngle = 120f;

    [Tooltip("Время в секундах, через которое NPC забудет цель, если потеряет её из виду")]
    public float targetLostDelay = 3f;

    [Header("Атака")]
    [Tooltip("Дистанция, с которой NPC начинает атаковать")]
    public float attackRange = 2f;

    [Tooltip("Урон одной атакой")]
    public float attackDamage = 15f;

    [Tooltip("Полная длительность анимации/состояния атаки")]
    public float attackDuration = 1.5f;

    [Tooltip("Задержка перед активацией хитбокса оружия внутри атаки")]
    public float attackWindup = 0.5f;

    [Tooltip("Минимальное время между атаками")]
    public float attackCooldown = 1.5f;
}
