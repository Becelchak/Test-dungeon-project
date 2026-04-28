using System.Collections.Generic;
using UnityEngine;

public interface IRoomSpawner
{
    void SpawnRoomObjects(GameObject roomInstance, RoomPrefab roomData, DungeonModifiers modifiers);
}

public class RoomSpawner : IRoomSpawner
{
    private readonly IResourceService _resources;
    public RoomSpawner(IResourceService resources) => _resources = resources;

    public void SpawnRoomObjects(GameObject roomInstance, RoomPrefab roomData, DungeonModifiers modifiers)
    {
        var spawnPoints = roomInstance.GetComponentsInChildren<RoomSpawnPoint>();
        foreach (var point in spawnPoints)
        {
            if (!point.mandatory && Random.value > GetSpawnChance(point, modifiers)) continue;
            var chosenPrefab = ChoosePrefab(point.possibleSpawns);
            if (chosenPrefab == null) continue;
            Instantiate(chosenPrefab, point.transform.position, point.transform.rotation);
        }
    }

    private float GetSpawnChance(RoomSpawnPoint point, DungeonModifiers modifiers)
    {
        // Здесь можно добавить логику на основе модификаторов
        return point.spawnType switch
        {
            SpawnType.Enemy => 0.6f * modifiers.enemiesMultiplier,
            SpawnType.Loot => 0.5f * modifiers.lootMultiplier,
            SpawnType.Trap => 0.3f * modifiers.trapMultiplier,
            _ => 0.8f
        };
    }

    private GameObject ChoosePrefab(List<SpawnEntry> entries)
    {
        // выбор на основе весов
    }
}