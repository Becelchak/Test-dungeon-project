using System.Collections.Generic;
using UnityEngine;

public class RoomComponent : MonoBehaviour
{
    public RoomPrefab data;
    public List<Transform> doorMarkers = new List<Transform>();          // автоматически собираются из дочерних объектов
    public List<RoomSpawnPoint> spawnPoints = new List<RoomSpawnPoint>();     // собираются аналогично
}