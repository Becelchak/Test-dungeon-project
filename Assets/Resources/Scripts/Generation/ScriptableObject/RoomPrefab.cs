using System.Collections.Generic;
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
        Calculate(); // Вызываем логику поиска из первого примера
        Debug.Log($"Размер для {name} обновлен: {size}");
    }

#if UNITY_EDITOR
    private void Calculate()
    {
        if (prefab == null) return;

        Transform cubeTransform = prefab.transform.Find("Geometry/Cube");
        if (cubeTransform == null) return;

        var pbMesh = cubeTransform.GetComponent<ProBuilderMesh>();
        if (pbMesh == null) return;

        // Используем Reflection, чтобы достать массив позиций вершин (m_Positions)
        // так как в вашей версии это поле private/protected
        FieldInfo positionsField = typeof(ProBuilderMesh).GetField("m_Positions", BindingFlags.Instance | BindingFlags.NonPublic);
        Vector3[] positions = (Vector3[])positionsField?.GetValue(pbMesh);

        if (positions != null && positions.Length > 0)
        {
            // Вычисляем границы (Bounds) вручную по вершинам
            Vector3 min = positions[0];
            Vector3 max = positions[0];

            for (int i = 1; i < positions.Length; i++)
            {
                min = Vector3.Min(min, positions[i]);
                max = Vector3.Max(max, positions[i]);
            }

            Vector3 localSize = max - min;
            // Учитываем масштаб объекта Cube
            Vector3 finalSize = Vector3.Scale(localSize, cubeTransform.localScale);

            if (Vector3.Distance(size, finalSize) > 0.001f)
            {
                size = finalSize;
                EditorUtility.SetDirty(this);
            }
        }
        else
        {
            // Если Reflection не сработал, пробуем вызвать внутренний метод обновления
            // pbMesh.Rebuild(); или pbMesh.ToMesh(); 
            // Но расчет по позициям — самый надежный для префабов в ассетах.
        }
    }
#endif
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