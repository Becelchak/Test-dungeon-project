using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomPrefab", menuName = "Dungeon/RoomPrefab")]
public class RoomPrefab : ScriptableObject
{
    public GameObject prefab;                         // Префаб сцен-объекта
    public RoomType roomType;                         // Start, Normal, Exit, Boss, Unique
    public float weight = 1f;                         // Вес при случайном выборе
    public List<RoomTag> requiredTags;                // Условия появления (например, "HasSword", "LowTrust")
    public List<RoomModifier> roomModifiers;          // Локальные модификаторы спавна
    public Vector2Int size = Vector2Int.one;          // Размер комнаты (для позиционирования)
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

}

public enum RoomTag
{

}