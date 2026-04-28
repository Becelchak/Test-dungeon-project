using UnityEngine;

public class DungeonModifiers
{
    [Tooltip("Модификатор количества врагов")]
    public float enemiesMultiplier = 1f;
    [Tooltip("Модификатор количества добычи")]
    public float lootMultiplier = 1f;
    [Tooltip("Модификатор количество ловушек")]
    public float trapMultiplier = 1f;
    [Tooltip("Модификатор дополнительных комнаты")]
    public float roomsCountMultiplier = 1f;
    [Tooltip("Появление \"элитных\" комнат")]
    public bool forceEliteRooms = false;
    [Tooltip("Включить ли боссу приспешников")]
    public bool forceBossWithMinions = false;
}