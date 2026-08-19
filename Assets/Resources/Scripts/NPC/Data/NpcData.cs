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

    [Tooltip("Минимальное время проводимое в состоянии стаггера")]
    public float minStaggerTime = 1f;

    [Tooltip("Максимальное время проводимое в состоянии стаггера")]
    public float maxStaggerTime = 2f;

    [Header("Тактика: блок")]
    [Tooltip("Шанс (0-1) начать блок, когда игрок атакует в ближней дистанции")]
    [Range(0f, 1f)]
    public float blockChance = 0.4f;

    [Tooltip("Доли урона, которые проходят через блок (0 = полный блок, 1 = блок не работает)")]
    [Range(0f, 1f)]
    public float blockDamageReduction = 0.5f;

    [Tooltip("Минимальное время между попытками блока")]
    public float blockCooldown = 2f;

    [Tooltip("Максимальная длительность удержания блока")]
    public float blockDuration = 1.5f;

    [Header("Тактика: обход")]
    [Tooltip("Шанс (0-1) начать обход, когда игрок атакует")]
    [Range(0f, 1f)]
    public float strafeChance = 0.3f;

    [Tooltip("Длительность обхода вокруг цели")]
    public float strafeDuration = 1.2f;

    [Tooltip("Множитель скорости при обходе")]
    public float strafeSpeedMultiplier = 0.9f;

    [Tooltip("Желаемая дистанция от цели при обходе")]
    public float strafeRadius = 2.5f;

    [Header("Тактика: отступление")]
    [Tooltip("Порог здоровья NPC (доля от максимума), при котором может сработать отступление")]
    [Range(0f, 1f)]
    public float retreatHealthThreshold = 0.3f;

    [Tooltip("Шанс (0-1) начать отступление при низком здоровье")]
    [Range(0f, 1f)]
    public float retreatChance = 0.5f;

    [Tooltip("Дистанция, на которую NPC отступает")]
    public float retreatDistance = 5f;

    [Tooltip("Множитель скорости при отступлении")]
    public float retreatSpeedMultiplier = 1.3f;

    [Header("Тактика: агрессия")]
    [Tooltip("Порог здоровья игрока (доля от максимума), ниже которого NPC переходит в агрессивное наступление")]
    [Range(0f, 1f)]
    public float aggressivePlayerHealthThreshold = 0.3f;

    [Tooltip("Шанс (0-1) перейти в агрессивное наступление, когда у игрока мало здоровья")]
    [Range(0f, 1f)]
    public float aggressiveChance = 0.6f;

    [Tooltip("Множитель скорости при агрессивном наступлении")]
    public float aggressiveSpeedMultiplier = 1.4f;
}
