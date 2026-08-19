using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.ProBuilder;
#endif

[CreateAssetMenu(fileName = "RoomPrefab", menuName = "Dungeon/RoomPrefab")]
public class RoomPrefab : ScriptableObject
{
    public GameObject prefab;
    public RoomType roomType;
    [Tooltip("Вес при случайном выборе")]
    public float weight = 1f;
    [Tooltip("Условия появления (например, \"HasSword\", \"LowTrust\")")]
    public List<RoomTag> requiredTags = new List<RoomTag>();
    [Tooltip("Локальные модификаторы спавна")]
    public List<RoomModifier> roomModifiers = new List<RoomModifier>();
    [Tooltip("Размеры комнаты (ширина, высота, глубина)")]
    public Vector3 size;
    [Tooltip("Смещение центра относительно корня (обычно (0,0,0))")]
    public Vector3 center;
    [ContextMenu("Calculate Size from Prefab")]
    public void CalculateSize()
    {
        Calculate();
        Debug.Log($"Размер для {name} обновлен: {size}");
    }

//#if UNITY_EDITOR
    private void Calculate()
    {
        if (prefab == null) return;
        Transform geometryTransform = prefab.transform.Find("Geometry");
        if (geometryTransform == null) return;

        Vector3 minPoint = Vector3.one * float.MaxValue;
        Vector3 maxPoint = Vector3.one * float.MinValue;
        bool hasFoundAnyVertex = false;

        var floors = geometryTransform.GetComponentsInChildren<Transform>()
                     .Where(c => c.CompareTag("GeometryFloor"));

        foreach (var child in floors)
        {
            var pbMesh = child.GetComponent<ProBuilderMesh>();
            if (pbMesh == null) continue;

            FieldInfo positionsField = typeof(ProBuilderMesh).GetField("m_Positions", BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3[] positions = (Vector3[])positionsField?.GetValue(pbMesh);

            if (positions != null && positions.Length > 0)
            {
                hasFoundAnyVertex = true;
                foreach (var pos in positions)
                {
                    // 1. Учитываем масштаб меша
                    Vector3 scaledPos = Vector3.Scale(pos, child.localScale);
                    // 2. Переводим в локальные координаты ПРЕФАБА (учитываем позицию и поворот дочернего объекта)
                    Vector3 worldPos = child.localPosition + (child.localRotation * scaledPos);

                    // 3. Обновляем общие границы
                    minPoint = Vector3.Min(minPoint, worldPos);
                    maxPoint = Vector3.Max(maxPoint, worldPos);
                }
            }
        }

        if (hasFoundAnyVertex)
        {
            size = maxPoint - minPoint;
            center = minPoint + (size / 2f); // Центр тоже очень важен для OverlapsAny!
        }

        EditorUtility.SetDirty(this);
    }
//#endif
}


public enum RoomType
{
    Start = 0,
    Normal = 1,
    Exit = 2,
    Boss = 3,
    Unique = 4,
}

public enum RoomModifier
{
    MoreEnemy = 0,
    MoreLoot = 1,
}

public enum RoomTag
{
    LowTrust = 0,
    HightTrust = 1,
}