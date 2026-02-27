using UnityEngine;

public interface ISpawnPoint
{
    SpawnType spawnType { get; }
    Vector3 orientation { get; }
    void SpawnStart();
    Vector3 GetSpawnPosition();
    Quaternion GetSpawnRotation();
}

public enum SpawnType
{
    Player = 0,
    Loot = 1,
    Wall = 2,
    Spikes = 3,

}
