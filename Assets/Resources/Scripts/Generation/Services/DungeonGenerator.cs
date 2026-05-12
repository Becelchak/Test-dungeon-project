using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour, IDungeonGenerator
{
    [Tooltip("Допуск на стыках комнат при проверки их пересечения")]
    [SerializeField] private float tolerance = 0.1f;
    private readonly IResourceService _resources;
    private System.Random random;
    public DungeonGenerator(IResourceService resources) => _resources = resources;

    [Serializable]
    public class RoomDebugData
    {
        public Bounds bounds;
        public Color color;
    }
    private List<RoomDebugData> debugRooms = new();

    public FloorPlan GenerateFloor(FloorSettings settings, GameConfig config)
    {
        random = new System.Random(settings.seed);
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

        //DEBUG
        debugRooms = new();
        // Расстановка позиций (линейная цепочка с возможностью ветвления)
        return BuildFloorPlan(allRooms);
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

    private FloorPlan BuildFloorPlan(List<RoomPrefab> rooms)
    {
        var instances = new List<RoomInstance>();
        List<(RoomInstance room, DoorMarker door)> freeDoors = new ();

        var start = new RoomInstance
        {
            data = rooms[0],
            position = Vector3.zero,
            rotation = Quaternion.identity,
            doorMarkers = GetDoorMarkers(rooms[0].prefab),
            spawnPoints = GetSpawnPoints(rooms[0].prefab)
        };
        instances.Add(start);
        RefreshFreeDoors(start, freeDoors);

        for (int i = 1; i < rooms.Count; i++)
        {
            bool placed = false;
            int attempts = 0;
            const int maxAttempts = 5;

            while (!placed && freeDoors.Count > 0 && attempts < maxAttempts)
            {
                var doorIndex = UnityEngine.Random.Range(0, Mathf.Max(0, freeDoors.Count));
                var (parentRoom, parentDoor) = freeDoors[doorIndex];
                var newRoomData = rooms[i];
                var newMarkers = GetDoorMarkers(newRoomData.prefab);
                var compatible = FindCompatibleMarker(parentDoor, newMarkers);
                if (compatible == null) 
                {
                    attempts++;
                    continue;
                } 

                var (newPos, newRot) = CalculateRoomPlacement(parentRoom, parentDoor, compatible);
                var newRoom = new RoomInstance
                {
                    data = newRoomData,
                    position = newPos,
                    rotation = newRot,
                    doorMarkers = newMarkers,
                    spawnPoints = GetSpawnPoints(newRoomData.prefab)
                };

                if (!OverlapsAny(newRoom, instances))
                {
                    instances.Add(newRoom);
                    parentDoor.SetDoorCloseStatus(true);
                    compatible.SetDoorCloseStatus(true);
                    RefreshFreeDoors(newRoom, freeDoors);
                    foreach (var room in instances)
                    {
                        MarkBlockedDoors(newRoom, room);
                    }
                    freeDoors.RemoveAll(item => item.door.IsClosedDoor);
                    placed = true;
                    break;
                }
                else
                {
                    attempts++;
                }

            }

            if (!placed)
            {
                Debug.LogWarning($"Не удалось разместить комнату {rooms[i].name}");
                continue;
            }
        }

        Debug.Log($"Размещено комнат: {instances.Count} из {rooms.Count}");
        return new FloorPlan(instances);
    }

    private void RefreshFreeDoors(RoomInstance room, List<(RoomInstance room, DoorMarker door)> list)
    {
        foreach (var d in room.doorMarkers)
        {
            if (d.IsClosedDoor == true) continue;
            list.Add((room, d));
        }
    }

    private List<DoorMarker> GetDoorMarkers(GameObject prefab)
    {
        var dm = prefab.GetComponentsInChildren<DoorMarker>(true);
        foreach (var d in dm)
            d.SetDoorCloseStatus(false);
        return new List<DoorMarker>(dm);
    }

    private List<RoomSpawnPoint> GetSpawnPoints(GameObject prefab)
    {
        return new List<RoomSpawnPoint>(prefab.GetComponentsInChildren<RoomSpawnPoint>(true));
    }

    private DoorMarker FindCompatibleMarker(DoorMarker parentDoor, List<DoorMarker> newMarkers)
    {
        return newMarkers
        .Where(m => m.Width == parentDoor.Width)
        .OrderBy(_ => Guid.NewGuid())
        .FirstOrDefault();
    }

    private Bounds GetWorldBounds(RoomInstance room)
    {
        Bounds localBounds = new Bounds(room.data.center, room.data.size);
        Vector3 min = localBounds.min;
        Vector3 max = localBounds.max;

        // Все 8 углов локального бокса
        Vector3[] corners = new Vector3[] {
        new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
        new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
        new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z),
        new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z)
    };

        // Трансформируем каждый угол в мировые координаты
        Bounds worldBounds = new Bounds(room.position + room.rotation * corners[0], Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
        {
            worldBounds.Encapsulate(room.position + room.rotation * corners[i]);
        }
        return worldBounds;
    }

    private bool OverlapsAny(RoomInstance newRoom, List<RoomInstance> existing)
    {
        Bounds newBounds = GetWorldBounds(newRoom);
        debugRooms.Add(new RoomDebugData { bounds = newBounds, color = Color.green });

        foreach (var room in existing)
        {
            Bounds otherBounds = GetWorldBounds(room);
            if (BoundsTouchOrOverlap(newBounds, otherBounds))
            { 
                debugRooms.Last().color = Color.red;
                return true;
            }    
                
        }
        return false;
    }

    private void MarkBlockedDoors(RoomInstance newRoom, RoomInstance oldRoom)
    {
        foreach (var doorMakers in oldRoom.doorMarkers)
        {
            var doorPoint = doorMakers.transform.position + doorMakers.transform.forward * 1f;
            var newRoomBound = GetWorldBounds(newRoom);
            if (newRoomBound.Contains(doorPoint))
                doorMakers.SetDoorCloseStatus(true);
        }
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
        Vector3 parentDoorWorldPos = parentRoom.position + (parentRoom.rotation * parentDoor.LocalPosition);
        Vector3 parentDoorWorldDir = parentRoom.rotation * parentDoor.Forward;
        parentDoorWorldDir.Normalize();

        // 2. Рассчитываем вращение новой комнаты
        // Нам нужно, чтобы childDoor.Forward смотрел ровно в противоположную сторону от parentDoorWorldDir
        Vector3 targetChildForward = -parentDoorWorldDir;

        // Вычисляем разницу между локальным форвардом двери ребенка и нужным нам направлением в мире
        Quaternion rotationOffset = Quaternion.FromToRotation(childDoor.Forward, targetChildForward);
        Quaternion childRotation = rotationOffset;

        // Убираем наклоны по X и Z (оставляем только Y поворот для подземелья)
        Vector3 euler = childRotation.eulerAngles;
        childRotation = Quaternion.Euler(0, euler.y, 0);

        // 3. Рассчитываем позицию новой комнаты
        // childDoorWorldPos должен совпасть с parentDoorWorldPos
        // Вычисляем, где окажется центр комнаты, если дверь будет в этой точке
        Vector3 childDoorLocalPos = childDoor.LocalPosition;
        Vector3 childPos = parentDoorWorldPos - (childRotation * childDoorLocalPos);

        return (childPos, childRotation);
    }

    private void OnDrawGizmos()
    {
        if (debugRooms == null) return;

        foreach (var debug in debugRooms)
        {
            Gizmos.color = debug.color;
            Gizmos.DrawWireCube(debug.bounds.center, debug.bounds.size);

            // Можно закрасить полупрозрачным
            Gizmos.color = new Color(debug.color.r, debug.color.g, debug.color.b, 0.2f);
            Gizmos.DrawCube(debug.bounds.center, debug.bounds.size);
        }
    }

}