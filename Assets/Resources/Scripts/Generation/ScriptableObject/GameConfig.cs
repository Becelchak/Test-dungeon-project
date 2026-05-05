using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int floorsCount = 3;
    public int bossFloor = 3;
    public Range roomsPerFloor = new Range(5, 10);
    public float roomSpacing = 20f;
    public List<RoomPrefab> startRooms;
    public List<RoomPrefab> normalRooms;
    public List<RoomPrefab> exitRooms;
    public List<RoomPrefab> bossRooms;
    public List<RoomPrefab> uniqueRooms;
    public FloorSettings floorSettings;
}