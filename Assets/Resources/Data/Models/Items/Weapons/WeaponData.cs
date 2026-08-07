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
    }

    private void OnValidate()
    {
        // Вызывается в редакторе при изменении полей. 
        // Помогает сразу увидеть изменения, если поменяли файл JSON.
        LoadDataFromJson();
    }
}