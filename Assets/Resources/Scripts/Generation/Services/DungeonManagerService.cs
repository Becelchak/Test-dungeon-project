using Cysharp.Threading.Tasks;
using EventBusSystem;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManagerService : BaseService, IDungeonManagerService
{
    //[SerializeField] private float _baseRespawnInterval;
    //[SerializeField] private float baseRespawnIntervalMin = 18f;
    //[SerializeField] private float baseRespawnIntervalMax = 21f;

    // Параметры, которые будет изменять ML-агент
    [SerializeField] public float wallMultiplier = 1f;
    [SerializeField] public float lootMultiplier = 1f;
    [SerializeField] public float trapMultiplier = 1f;
    [SerializeField] public float respawnMultiplier = 1f;
    [SerializeField] public float goldMultiplier = 1f;

    private int _requiredGold;
    private bool _levelCompleted = false;
    private bool _respawnEnabled = true;

    public int RequiredGold => _requiredGold;
    public bool IsLevelCompleted => _levelCompleted;
    public bool _isInitialized = false;

    //[Header("Spawn Points")]
    //[SerializeField] private GameObject spawnPointsObject;
    //[SerializeField] private List<SpawnPoint> allSpawnPoints;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] lootPrefabs;
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject[] spikePrefabs;

    [Header("Thresholds")]
    [SerializeField] private int minLoot = 1;
    [SerializeField] private int minWalls = 1;
    [SerializeField] private int minSpikes = 1;

    [Header("Dungeon References")]
    [SerializeField] private Transform dungeonContainer;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private RoomSpawner roomSpawner;
    [SerializeField] private FloorSettings settings;

    private Dictionary<RoomInstance, GameObject> _roomObjects = new Dictionary<RoomInstance, GameObject>();
    private FloorPlan _currentPlan;


    private void Awake()
    {
        base.Awake();
        //dungeonGenerator = (DungeonGenerator) ServiceLocator.Instance.GetService<IDungeonGenerator>();
    }

    private async void LoadModifiersFromProfile()
    {
        await UniTask.WaitForEndOfFrame();
    }

    public void InitializeDungeon()
    {

    }

    private void Start()
    {
        //LoadModifiersFromProfile();
        //var settings = new FloorSettings
        //{
        //    floorIndex = 1,
        //    targetRoomsCount = 5,
        //    mlModifiers = new DungeonModifiers(),
        //    difficultyMultiplier = 1,
        //    seed = Random.
        //    (0, int.MaxValue)
        //};
        //await UniTask.SwitchToMainThread();
        var plan = dungeonGenerator.GenerateFloor(settings, gameConfig);
        SpawnDungeon(plan);
        SpawnPlayer(plan);
    }


    private void SpawnDungeon(FloorPlan plan)
    {
        _currentPlan = plan;
        _roomObjects.Clear();

        // 1. Создаём все комнаты
        foreach (var room in plan.rooms)
        {
            var roomObj = Instantiate(room.data.prefab, dungeonContainer);
            roomObj.transform.position = room.position;
            roomObj.transform.rotation = room.rotation;
            roomObj.name = $"{room.data.name}_{room.position}";

            // Сохраняем связь для дальнейшего доступа
            _roomObjects[room] = roomObj;

            // 2. Заполняем комнату содержимым (через RoomComponent и RoomSpawner)
            var roomComponent = roomObj.GetComponentInChildren<RoomComponent>();
            if (roomComponent != null)
            {
                roomSpawner.SpawnRoomObjects(roomComponent, room.data, gameConfig.floorSettings.mlModifiers);
            }
        }

        var exitInstance = _roomObjects.Keys.FirstOrDefault(room => room.data != null && room.data.roomType == RoomType.Exit);
        if (exitInstance == null)
        {
            var roomInstance = _roomObjects.Keys.LastOrDefault();
            var roomComponent = _roomObjects[roomInstance].GetComponentInChildren<RoomComponent>();
            roomSpawner.SpawnRoomObjects(roomComponent, roomInstance.data, gameConfig.floorSettings.mlModifiers, SpawnType.Exit);
        }

        // 3. Активируем визуальные соединения (опционально)
        SetupDoorConnections(plan);
    }

    private void SetupDoorConnections(FloorPlan plan)
    {
        foreach (var connection in plan.connections)
        {
            var roomAObj = _roomObjects[connection.roomA];
            var roomBObj = _roomObjects[connection.roomB];

            // Находим маркеры дверей в инстанциированных комнатах
            var doorA = roomAObj.transform.Find(connection.doorMarkerA.gameObject.name);
            var doorB = roomBObj.transform.Find(connection.doorMarkerB.gameObject.name);

            // Можно добавить визуальный эффект (свет, партиклы) или просто залогировать
            Debug.Log($"Connected {roomAObj.name} -> {roomBObj.name}");

        }
    }

    private void SpawnPlayer(FloorPlan plan)
    {
        var player = (PlayerMovementService) ServiceLocator.Instance.GetService<IPlayerMovementService>();
        var spawnPoint = plan.rooms[plan.startRoomIndex];

        var rb = player.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            var spawnPointsList = spawnPoint.spawnPoints.Where(point => point.spawnType == SpawnType.Player).ToList();
            var point = spawnPointsList[Random.Range(0, spawnPointsList.Count - 1)];
            player.gameObject.transform.position = point.GetSpawnPosition();
            player.gameObject.transform.rotation = point.GetSpawnRotation();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //Debug.Log($"After {player.transform.position}");
    }


    protected override System.Type GetServiceType() => typeof(IDungeonManagerService);
}
