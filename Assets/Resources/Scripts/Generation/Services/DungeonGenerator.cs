using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour, IDungeonGenerator
{
    [Tooltip("Допуск на стыках комнат при проверки их пересечения")]
    [SerializeField] private float tolerance = 0.1f;
    private readonly IResourceService _resources;
    public DungeonGenerator(IResourceService resources) => _resources = resources;

    public FloorPlan GenerateFloor(FloorSettings settings, GameConfig config)
    {
        var random = new System.Random(settings.seed);
        var roomPool = GetRoomPoolByType(config, settings);

        var startRoom = GetStartRoom(config, random);
        var normalRoomCount = UnityEngine.Mathf.Max(0, settings.targetRoomsCount - 2);
        var normalRooms = Enumerable.Range(0, normalRoomCount)
            .Select(_ => ChooseRoom(roomPool.normalRooms, random))
            .ToList();

        // Определяем комнату выхода или босса
        RoomPrefab exitOrBossRoom;
        if (settings.floorIndex == config.bossFloor)
        {
            Debug.Log("BOSS TIME");
            exitOrBossRoom = ChooseRoom(roomPool.bossRooms, random);
        }
        else
            exitOrBossRoom = ChooseRoom(roomPool.exitRooms, random);

        var allRooms = new List<RoomPrefab> { startRoom };
        allRooms.AddRange(normalRooms);
        allRooms.Add(exitOrBossRoom);
        // Расстановка позиций (линейная цепочка с возможностью ветвления)
        return BuildFloorPlan(allRooms, config.roomSpacing);
    }

    private RoomPrefab GetStartRoom(GameConfig config, System.Random random)
    {
        return ChooseRoom(config.startRooms, random);
    }

    private (List<RoomPrefab> normalRooms, List<RoomPrefab> exitRooms, List<RoomPrefab> bossRooms, List<RoomPrefab> uniqueRooms)
        GetRoomPoolByType(GameConfig config, FloorSettings settings)
    {
        return (
            normalRooms: config.normalRooms,
            exitRooms: config.exitRooms,
            bossRooms: config.bossRooms,
            uniqueRooms: config.uniqueRooms
        );
    }

    private RoomPrefab ChooseRoom(List<RoomPrefab> pool, System.Random random)
    {
        if(pool.Count == 0) return null;
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
        var freeDoors = new Queue<(RoomInstance room, DoorMarker door)>();

        // 1. Стартовая комната
        var start = new RoomInstance
        {
            data = rooms[0],
            position = Vector3.zero,
            rotation = Quaternion.identity,
            doorMarkers = GetDoorMarkers(rooms[0].prefab),
            spawnPoints = GetSpawnPoints(rooms[0].prefab)
        };
        instances.Add(start);
        EnqueueFreeDoors(start, freeDoors);

        int currentRoomIndex = 1; // текущая комната для размещения
        int maxAttempts = 10;     // максимум попыток на одну комнату
        int attempts = 0;

        while (currentRoomIndex < rooms.Count && freeDoors.Count > 0 && attempts < maxAttempts * rooms.Count)
        {
            var (parentRoom, parentDoor) = freeDoors.Dequeue();
            var newRoomData = rooms[currentRoomIndex];
            var newRoomMarkers = GetDoorMarkers(newRoomData.prefab);
            var compatibleMarker = FindCompatibleMarker(parentDoor, newRoomMarkers);

            if (compatibleMarker == null)
            {
                // эта дверь не подходит – пробуем другую
                continue;
            }

            var (newPos, newRot) = CalculateRoomPlacement(parentRoom, parentDoor, compatibleMarker);
            var newRoom = new RoomInstance
            {
                data = newRoomData,
                position = newPos,
                rotation = newRot,
                doorMarkers = newRoomMarkers,
                spawnPoints = GetSpawnPoints(newRoomData.prefab)
            };

            if (!OverlapsAny(newRoom, instances))
            {
                // Успешное размещение
                instances.Add(newRoom);
                EnqueueFreeDoors(newRoom, freeDoors, skipDoor: compatibleMarker);
                currentRoomIndex++;
                attempts = 0;
            }
            else
            {
                // Эта дверь ведёт к наложению – возвращаем её в конец очереди и пробуем другую
                freeDoors.Enqueue((parentRoom, parentDoor));
                attempts++;
            }
        }

        Debug.Log($"Размещено комнат: {instances.Count} из {rooms.Count}");
        return new FloorPlan(instances);
    }

    private void EnqueueFreeDoors(RoomInstance room, Queue<(RoomInstance, DoorMarker)> queue, DoorMarker skipDoor = null)
    {
        foreach (var door in room.doorMarkers)
        {
            if (door == skipDoor) continue;
            queue.Enqueue((room, door));
        }
    }

    private List<DoorMarker> GetDoorMarkers(GameObject prefab)
    {
        // Временное решение: ищем все DoorMarker в префабе (включая неактивные)
        return new List<DoorMarker>(prefab.GetComponentsInChildren<DoorMarker>(true));
    }

    private List<RoomSpawnPoint> GetSpawnPoints(GameObject prefab)
    {
        return new List<RoomSpawnPoint>(prefab.GetComponentsInChildren<RoomSpawnPoint>(true));
    }

    private DoorMarker FindCompatibleMarker(DoorMarker parentDoor, List<DoorMarker> newMarkers)
    {
        return newMarkers.FirstOrDefault(m => m.Side == DoorConnection.OppositeSide(parentDoor.Side));
    }

    private Bounds GetWorldBounds(RoomInstance room)
    {
        Vector3 size = room.data.size;
        Vector3 center = room.data.center;
        // Применяем поворот к центру (просто умножаем)
        Vector3 worldCenter = room.position + room.rotation * center;
        // Размеры: для поворота на 90° меняем X и Z местами
        Vector3 worldSize = size;
        if (Mathf.Abs(room.rotation.eulerAngles.y % 180 - 90) < 0.1f)
        {
            worldSize = new Vector3(size.z, size.y, size.x);
        }
        return new Bounds(worldCenter, worldSize);
    }

    private bool OverlapsAny(RoomInstance newRoom, List<RoomInstance> existing)
    {
        Bounds newBounds = GetWorldBounds(newRoom);
        foreach (var room in existing)
        {
            Bounds otherBounds = GetWorldBounds(room);
            return BoundsTouchOrOverlap(newBounds, otherBounds);
        }
        return false;
    }

    private bool BoundsTouchOrOverlap(Bounds a, Bounds b)
    {
        Vector3 dist = a.center - b.center;
        Vector3 safeExtSum = a.extents + b.extents - Vector3.one * tolerance;
        return Mathf.Abs(dist.x) < safeExtSum.x &&
               Mathf.Abs(dist.y) < safeExtSum.y &&
               Mathf.Abs(dist.z) < safeExtSum.z;
    }

    private (Vector3 pos, Quaternion rot) CalculateRoomPlacement(
        RoomInstance parentRoom,
        DoorMarker parentDoor,
        DoorMarker childDoor)
    {
        Vector3 parentDoorWorldPos = parentRoom.position + parentRoom.rotation * parentDoor.LocalPosition;
        Vector3 parentDoorWorldDir = parentRoom.rotation * parentDoor.Forward;

        Quaternion childRotation = Quaternion.FromToRotation(childDoor.Forward, -parentDoorWorldDir) * parentRoom.rotation;
        Vector3 childDoorWorldPos = parentDoorWorldPos;
        Vector3 childDoorLocalPos = childDoor.LocalPosition;
        Vector3 childPos = childDoorWorldPos - (childRotation * childDoorLocalPos);

        return (childPos, childRotation);
    }

}