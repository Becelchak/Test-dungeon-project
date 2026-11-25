using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сохраняемое состояние экспедиции (подземелья/рейда).
/// Хранит сгенерированный план целиком — генератор недетерминирован,
/// поэтому seed недостаточен для восстановления.
/// Углы поворота хранятся как эйлеровы (JsonUtility не сериализует Quaternion).
/// </summary>
[Serializable]
public class ExpeditionState
{
    public bool isActive = true;
    public int floorIndex;
    public int seed;
    public int startRoomIndex;
    public int exitRoomIndex;
    public List<ExpeditionRoomState> rooms = new List<ExpeditionRoomState>();

    /// <summary>Комната, в которой находится игрок (-1 — неизвестно).</summary>
    public int playerRoomId = -1;
    public Vector3 playerPosition;
    public Vector3 playerRotationEuler;
}

[Serializable]
public class ExpeditionRoomState
{
    /// <summary>Индекс комнаты в FloorPlan.rooms — стабильный id.</summary>
    public int roomId;
    /// <summary>Имя ассета RoomPrefab (ScriptableObject.name) для восстановления ссылки.</summary>
    public string prefabName;
    public Vector3 position;
    public Vector3 rotationEuler;
    public int roomType;
    /// <summary>Комната зачищена (все враги убиты) — содержимое не респавнится.</summary>
    public bool cleared;
}
