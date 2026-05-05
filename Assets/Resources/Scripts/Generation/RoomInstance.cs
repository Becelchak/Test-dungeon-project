using System.Collections.Generic;
using UnityEngine;

public class RoomInstance
{
    public RoomPrefab data;
    public Vector3 position;
    public Quaternion rotation;
    public List<DoorMarker> doorMarkers;
    public List<RoomSpawnPoint> spawnPoints;
}