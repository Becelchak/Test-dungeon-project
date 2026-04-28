using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : IDungeonGenerator
{
    private readonly IResourceService _resources;
    public DungeonGenerator(IResourceService resources) => _resources = resources;

    public FloorPlan GenerateFloor(FloorSettings settings, GameConfig config)
    {
        Random random = new Random(settings.seed);
        var roomPool = GetRoomPoolByType(config, settings);

        var startRoom = GetStartRoom(roomPool, random);
        var normalRoomCount = Mathf.Max(0, settings.targetRoomsCount - 2); // минус старт и выход/босс
        var normalRooms = Enumerable.Range(0, normalRoomCount)
            .Select(_ => ChooseRoom(roomPool.normalRooms, random))
            .ToList();

        // ќпредел€ем комнату выхода или босса
        RoomPrefab exitOrBossRoom;
        if (settings.floorIndex == config.bossFloor)
            exitOrBossRoom = ChooseRoom(roomPool.bossRooms, random);
        else
            exitOrBossRoom = ChooseRoom(roomPool.exitRooms, random);

        var allRooms = new[] { startRoom }.Concat(normalRooms).Append(exitOrBossRoom).ToList();
        // –асстановка позиций (линейна€ цепочка с возможностью ветвлени€)
        return BuildFloorPlan(allRooms, config.roomSpacing);
    }

    private RoomPrefab ChooseRoom(List<RoomPrefab> pool, Random random)
    {
        // ¬ыбор с учЄтом весов
        float totalWeight = pool.Sum(r => r.weight);
        float roll = (float)random.NextDouble() * totalWeight;
        foreach (var room in pool)
        {
            if (roll < room.weight) return room;
            roll -= room.weight;
        }
        return pool[0];
    }

    private FloorPlan BuildFloorPlan(List<RoomPrefab> rooms, float spacing)
    {
        var instances = new List<RoomInstance>();
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < rooms.Count; i++)
        {
            instances.Add(new RoomInstance
            {
                data = rooms[i],
                position = pos,
                rotation = Quaternion.identity,
                connections = new List<int>() // здесь можно заполнить св€зи
            });
            pos += Vector3.right * spacing;
        }
        return new FloorPlan(instances);
    }
}