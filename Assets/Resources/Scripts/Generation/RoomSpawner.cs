using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IRoomSpawner
{
    void SpawnRoomObjects(RoomComponent roomInstance, RoomPrefab roomData, DungeonModifiers modifiers, SpawnType? prefereSpawnPointType = null);
}

public class RoomSpawner : MonoBehaviour ,IRoomSpawner
{
    private readonly IResourceService _resources;
    public RoomSpawner(IResourceService resources) => _resources = resources;

    public void SpawnRoomObjects(RoomComponent roomInstance, RoomPrefab roomData, DungeonModifiers modifiers, SpawnType? prefereSpawnPointType = null)
    {
        var spawnPoints = roomInstance.spawnPoints;
        if (prefereSpawnPointType != null)
            spawnPoints.RemoveAll(p => p.spawnType != prefereSpawnPointType);
        foreach (var point in spawnPoints)
        {
            if (!point.mandatory && Random.value > GetSpawnChance(point, modifiers, prefereSpawnPointType)) continue;
            var chosenPrefab = ChoosePrefab(point.possibleSpawns);
            if (chosenPrefab == null) continue;
            var rObj = Instantiate(chosenPrefab, point.transform.position, point.transform.rotation);
        }
    }

    private float GetSpawnChance(RoomSpawnPoint point, DungeonModifiers modifiers, SpawnType? prefereSpawnPointType = null)
    {
        // Здесь можно добавить логику на основе модификаторов
        return point.spawnType switch
        {
            SpawnType.Enemy => 0.6f * modifiers.enemiesMultiplier,
            SpawnType.Loot => 0.5f * modifiers.lootMultiplier,
            SpawnType.Trap => 0.3f * modifiers.trapMultiplier,
            SpawnType.Exit => (prefereSpawnPointType == SpawnType.Exit) ? 1f : 0f,
            _ => 0.8f
        };
    }

    private GameObject ChoosePrefab(List<SpawnEntry> entries)
    {
        if (entries == null || entries.Count == 0) return null;

        // 1. Вычисляем общую сумму всех весов
        int totalWeight = 0;
        foreach (var entry in entries)
        {
            // Игнорируем отрицательные веса, если они вдруг есть
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0) return null;

        // 2. Выбираем случайное число в диапазоне [0, totalWeight)
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // 3. Проходим по списку и вычитаем вес каждого элемента из случайного числа
        // Тот элемент, на котором число упадет до нуля или ниже — наш выбор
        foreach (var entry in entries)
        {
            if (randomValue < entry.weight)
            {
                return entry.prefab;
            }
            randomValue -= entry.weight;
        }

        return entries[0].prefab; // Запасной вариант
    }
}