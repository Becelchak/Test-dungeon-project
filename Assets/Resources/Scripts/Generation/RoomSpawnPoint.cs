using System.Collections.Generic;
using UnityEngine;

public class RoomSpawnPoint : SpawnPoint
{
    public List<SpawnEntry> possibleSpawns;
    public bool mandatory = false;
    public DifficultyTier minDifficultyTier;
}
