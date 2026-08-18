using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/WeaponData")]
public class WeaponData : ItemData
{
    [Header("Visual & References")]
    public WeaponType weaponType;
    public WeaponHandling handling;
    [Tooltip("Именная анимация/анимации атаки")]
    public List<AnimationClip> attackAnimationClips;
    [Tooltip("Модель для отображения в руке")]
    public GameObject weaponPrefab;

    [Header("Weapon Holder Offset")]
    [Tooltip("Смещение модели оружия внутри WeaponHolder. " +
             "Позволяет сдвинуть/повернуть клеймор так, чтобы левая рука не тянулась через тело.")]
    public Vector3 weaponHolderOffset;
    [Tooltip("Поворот модели оружия внутри WeaponHolder (Euler).")]
    public Vector3 weaponHolderRotationOffsetEuler;

    [Header("Attack Weapon Holder Offset")]
    [Tooltip("Смещение модели оружия внутри WeaponHolder только на время атаки.")]
    public Vector3 weaponHolderAttackOffset;
    [Tooltip("Поворот модели оружия внутри WeaponHolder только на время атаки (Euler).")]
    public Vector3 weaponHolderAttackRotationOffsetEuler;

    [Header("Block Animation")]
    [Tooltip("Анимация блока данным оружием/щитом. Если не назначена — используется базовая.")]
    public AnimationClip blockAnimationClip;

    [Header("Parry Animation")]
    [Tooltip("Анимация парирования данным оружием/щитом. Если не назначена — используется базовая.")]
    public AnimationClip parryAnimationClip;

    [Header("Hit Animation")]
    [Tooltip("Анимация получения урона данным оружием/щитом. Если не назначена — используется базовая.")]
    public AnimationClip hitAnimationClip;

    [Header("Death Animation")]
    [Tooltip("Анимация смерти с этим оружием. Если не назначена — используется базовая.")]
    public AnimationClip deathAnimationClip;

    [Header("Grip IK Offsets")]
    [Tooltip("Смещение правой руки относительно RightGrip. " +
             "Используется, если включен Use Right Hand IK в WeaponIKController.")]
    public Vector3 rightHandPositionOffset;
    [Tooltip("Поворот правой руки относительно RightGrip (Euler).")]
    public Vector3 rightHandRotationOffsetEuler;
    [Tooltip("Смещение левой руки относительно LeftGrip. " +
             "Позволяет поправить положение ладони для двуручного оружия.")]
    public Vector3 leftHandPositionOffset;
    [Tooltip("Поворот левой руки относительно LeftGrip (Euler). " +
             "Позволяет поправить 'сломанную' ладонь для двуручного оружия.")]
    public Vector3 leftHandRotationOffsetEuler;
    public Vector3 twoHandedAttackPositionOffset;

    [Header("Aiming")]
    [Tooltip("Локальная ось оружия, вокруг которой выполняется вертикальный навод (pitch). " +
             "Обычно Vector3.right для клинков, направленных вперёд по Z.")]
    public Vector3 aimRotationAxis = Vector3.right;
    [Tooltip("Смещение pivot'а прицеливания относительно трансформа оружия. " +
             "Если модель оружия не отцентрована по хвату, задай точку хвата.")]
    public Vector3 aimPivotOffset = Vector3.zero;
    [Tooltip("Максимальный угол подъёма оружия при прицеливании.")]
    public float maxAimAngleUp = 60f;
    [Tooltip("Максимальный угол опускания оружия при прицеливании.")]
    public float maxAimAngleDown = 45f;
    [Tooltip("Скорость сглаживания прицеливания (градусов в секунду).")]
    public float aimSpeed = 180f;
    [Tooltip("Слои для raycast курсора: враги + земля. Должны исключать коллайдеры игрока.")]
    public LayerMask aimLayers;

    [Header("Data Source (JSON)")]
    [Tooltip("Ссылка на JSON файл в проекте")]
    [SerializeField] private TextAsset jsonFile;

    [Header("Runtime Stats (Loaded from JSON)")]
    [SerializeField] private WeaponStats stats;

    [Tooltip("Публичное свойство для безопасного доступа к характеристикам из других скриптов")]
    public WeaponStats Stats => stats;

    /// <summary>
    /// Загружает (или перезагружает) данные из прикрепленного JSON файла.
    /// </summary>
    [ContextMenu("Load Stats From JSON")] // Позволяет вызвать метод через ПКМ по компоненту в инспекторе
    public void LoadDataFromJson()
    {
        if (jsonFile == null)
        {
            Debug.LogWarning($"[WeaponData] JSON файл не назначен в ассете: {name}");
            return;
        }

        try
        {
            // Десериализуем данные из текста JSON в класс характеристик
            stats = JsonUtility.FromJson<WeaponStats>(jsonFile.text);
            Debug.Log($"[WeaponData] Данные для {name} успешно загружены из JSON.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WeaponData] Ошибка при чтении JSON в ассете {name}: {e.Message}");
        }
    }

    // Автоматическая загрузка данных при запуске игры или изменении в инспекторе
    private void OnEnable()
    {
        LoadDataFromJson();
        SyncItemId();
    }

    private void OnValidate()
    {
        // Вызывается в редакторе при изменении полей. 
        // Помогает сразу увидеть изменения, если поменяли файл JSON.
        LoadDataFromJson();
        SyncItemId();
    }

    /// <summary>
    /// Если глобальный itemId не задан, берёт его из JSON-поля weaponId.
    /// Это позволяет не дублировать ID вручную.
    /// </summary>
    private void SyncItemId()
    {
        if (string.IsNullOrWhiteSpace(itemId) && stats != null && !string.IsNullOrWhiteSpace(stats.weaponId))
        {
            itemId = stats.weaponId;
        }
    }
}