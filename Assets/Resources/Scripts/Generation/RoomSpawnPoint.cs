using System.Collections.Generic;
using UnityEngine;

public class RoomSpawnPoint : SpawnPoint
{
    public List<SpawnEntry> possibleSpawns;
    public bool mandatory = false;
    public DifficultyTier minDifficultyTier;
}


public enum DifficultyTier
{
    T1 = 0,
    T2 = 1,
    T3 = 2,
}