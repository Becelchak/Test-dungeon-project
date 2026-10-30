using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/WeaponData")]
public class WeaponData : ItemData
{
    [Header("Visual & References")]
    public WeaponType weaponType;
    public WeaponHandling handling;
    [Tooltip("Именная анимация/анимации атаки")]
    public List<AttackDefinition> attacks;
    [Tooltip("Модель для отображения в руке")]
    public GameObject weaponPrefab;

    [Header("Blade Direction")]
    [Tooltip("Направление КЛИНКА в локальных координатах префаба оружия. " +
             "Обычно (0, 1, 0) если модель 'лежит' клинком вверх, или (0, 0, 1) если клинок вперёд по Z.")]
    public Vector3 bladeLocalDirection = Vector3.up;

    [Header("Weapon Holder Offset (Idle)")]
    [Tooltip("Смещение модели оружия внутри WeaponHolder в Idle.")]
    public Vector3 weaponHolderOffset;
    [Tooltip("Поворот модели оружия внутри WeaponHolder в Idle (Euler).")]
    public Vector3 weaponHolderRotationOffsetEuler;

    [Header("Weapon Holder Offset (Attack)")]
    [Tooltip("Смещение модели оружия внутри WeaponHolder на время атаки.")]
    public Vector3 weaponHolderAttackOffset;
    [Tooltip("Поворот модели оружия внутри WeaponHolder на время атаки (Euler).")]
    public Vector3 weaponHolderAttackRotationOffsetEuler;

    [Header("Animation Clips")]
    public AnimationClip blockAnimationClip;
    public AnimationClip parryAnimationClip;
    public AnimationClip hitAnimationClip;
    public AnimationClip deathAnimationClip;

    [Header("Grip IK Offsets")]
    [Tooltip("Смещение правой руки относительно RightGrip.")]
    public Vector3 rightHandPositionOffset;
    [Tooltip("Поворот правой руки относительно RightGrip (Euler).")]
    public Vector3 rightHandRotationOffsetEuler;
    [Tooltip("Смещение левой руки относительно LeftGrip.")]
    public Vector3 leftHandPositionOffset;
    [Tooltip("Поворот левой руки относительно LeftGrip (Euler).")]
    public Vector3 leftHandRotationOffsetEuler;
    public Vector3 twoHandedAttackPositionOffset;

    [Header("Data Source (JSON)")]
    [SerializeField] private TextAsset jsonFile;

    [Header("Runtime Stats (Loaded from JSON)")]
    [SerializeField] private WeaponStats stats;

    public WeaponStats Stats => stats;

    [ContextMenu("Load Stats From JSON")]
    public void LoadDataFromJson()
    {
        if (jsonFile == null)
        {
            Debug.LogWarning($"[WeaponData] JSON файл не назначен в ассете: {name}");
            return;
        }

        try
        {
            stats = JsonUtility.FromJson<WeaponStats>(jsonFile.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WeaponData] Ошибка чтения JSON в {name}: {e.Message}");
        }
    }

    private void OnEnable()
    {
        LoadDataFromJson();
        SyncItemId();
    }

    private void OnValidate()
    {
        LoadDataFromJson();
        SyncItemId();
        
        if (bladeLocalDirection.sqrMagnitude < 0.0001f)
        {
            bladeLocalDirection = Vector3.up;
            Debug.LogWarning($"[WeaponData] bladeLocalDirection не настроен в {name}, установлен в Vector3.up");
        }
    }

    private void SyncItemId()
    {
        if (string.IsNullOrWhiteSpace(itemId) && stats != null && !string.IsNullOrWhiteSpace(stats.weaponId))
            itemId = stats.weaponId;
    }
}