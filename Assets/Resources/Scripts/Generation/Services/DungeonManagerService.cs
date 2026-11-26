using Cysharp.Threading.Tasks;
using EventBusSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет подземельем: генерация/восстановление плана этажа,
/// спавн комнат и их содержимого, трекинг зачищенных комнат
/// и сохранение состояния экспедиции через ISaveModule (секция "expedition").
/// </summary>
public class DungeonManagerService : BaseService, IDungeonManagerService, ISaveModule, IEnemyKilledEventSubscriber, IPlayerDiedEventSubscriber
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

    // ===== Состояние экспедиции =====
    private ISaveService _saveService;
    private ISaveService SaveService => _saveService ??= ServiceLocator.Instance.GetService<ISaveService>();
    private ExpeditionState _restoredState;
    private readonly HashSet<int> _clearedRooms = new HashSet<int>();
    private readonly Dictionary<int, HashSet<GameObject>> _roomEnemies = new Dictionary<int, HashSet<GameObject>>();
    private bool _expeditionEnded;
    private Dictionary<string, RoomPrefab> _roomPrefabByName;
    private float _roomCheckTimer;


    private void Awake()
    {
        base.Awake();
        //dungeonGenerator = (DungeonGenerator) ServiceLocator.Instance.GetService<IDungeonGenerator>();
        SaveService?.RegisterModule(this);
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
        // Повторная регистрация — на случай, если SaveService ещё не был доступен в Awake
        SaveService?.RegisterModule(this);
        EventBus.Subscribe(this);

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

        FloorPlan plan = RestorePlanFromSave() ?? GenerateNewPlan();
        SpawnDungeon(plan);
        SpawnPlayer(plan);
    }

    private void Update()
    {
        // Периодическая проверка: игрок вошёл в Exit-комнату → рейд пройден
        if (_expeditionEnded || _currentPlan == null) return;

        _roomCheckTimer -= Time.deltaTime;
        if (_roomCheckTimer > 0f) return;
        _roomCheckTimer = 0.25f;

        var player = ServiceLocator.Instance.GetService<IPlayerMovementService>() as PlayerMovementService;
        if (player == null) return;

        int roomId = FindRoomIdAt(player.transform.position);
        if (roomId < 0) return;

        var room = _currentPlan.rooms.FirstOrDefault(r => r.id == roomId);
        if (room?.data != null && (room.data.roomType == RoomType.Exit || roomId == _currentPlan.exitRoomIndex))
            CompleteExpedition();
    }

    private FloorPlan GenerateNewPlan()
    {
        // Новый заход в подземелье —previous expedition state сбрасываем
        _restoredState = null;
        _expeditionEnded = false;

        // Рандомизируем seed, чтобы каждый новый заход генерировал новое подземелье
        if (settings != null)
            settings.seed = UnityEngine.Random.Range(0, int.MaxValue);

        return dungeonGenerator.GenerateFloor(settings, gameConfig);
    }

    /// <summary>
    /// Пытается восстановить FloorPlan из сохранённой экспедиции.
    /// </summary>
    private FloorPlan RestorePlanFromSave()
    {
        var state = _restoredState;
        if (state == null && SaveService != null && SaveService.TryGetSection(Key, out var json))
            state = DeserializeState(json);

        if (state == null || !state.isActive || state.rooms == null || state.rooms.Count == 0)
            return null;

        var plan = BuildPlanFromState(state);
        if (plan == null || plan.rooms.Count == 0)
        {
            Debug.LogWarning("[Dungeon] Не удалось восстановить экспедицию из сохранения — генерируется новое подземелье");
            return null;
        }

        _restoredState = state;
        _expeditionEnded = false;
        Debug.Log($"[Dungeon] Экспедиция восстановлена: {plan.rooms.Count} комнат, игрок в комнате {state.playerRoomId}");
        return plan;
    }

    private ExpeditionState DeserializeState(string json)
    {
        try
        {
            return JsonUtility.FromJson<ExpeditionState>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Dungeon] Ошибка десериализации экспедиции: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Собирает FloorPlan из сохранённых комнат. Комнаты резолвятся по имени RoomPrefab.
    /// </summary>
    private FloorPlan BuildPlanFromState(ExpeditionState state)
    {
        EnsureRoomPrefabCache();

        var instances = new List<RoomInstance>();
        foreach (var roomState in state.rooms)
        {
            if (!_roomPrefabByName.TryGetValue(roomState.prefabName, out var data) || data == null || data.prefab == null)
            {
                Debug.LogWarning($"[Dungeon] RoomPrefab '{roomState.prefabName}' не найден — комната {roomState.roomId} пропущена");
                continue;
            }

            var instance = new RoomInstance
            {
                id = roomState.roomId,
                data = data,
                position = roomState.position,
                rotation = Quaternion.Euler(roomState.rotationEuler),
                doorMarkers = new List<DoorMarker>(),
                spawnPoints = new List<RoomSpawnPoint>(),
            };
            instances.Add(instance);

            if (roomState.cleared)
                _clearedRooms.Add(roomState.roomId);
        }

        var plan = new FloorPlan(instances)
        {
            startRoomIndex = Mathf.Clamp(state.startRoomIndex, 0, Mathf.Max(0, instances.Count - 1)),
            exitRoomIndex = Mathf.Clamp(state.exitRoomIndex, 0, Mathf.Max(0, instances.Count - 1)),
        };
        return plan;
    }

    private void EnsureRoomPrefabCache()
    {
        if (_roomPrefabByName != null) return;

        _roomPrefabByName = new Dictionary<string, RoomPrefab>();
        foreach (var prefab in Resources.LoadAll<RoomPrefab>(""))
        {
            if (prefab != null && !_roomPrefabByName.ContainsKey(prefab.name))
                _roomPrefabByName[prefab.name] = prefab;
        }
        Debug.Log($"[Dungeon] Закешировано {_roomPrefabByName.Count} RoomPrefab для восстановления экспедиции");
    }


    private void SpawnDungeon(FloorPlan plan)
    {
        _currentPlan = plan;
        _roomObjects.Clear();
        _roomEnemies.Clear();

        // 1. Создаём все комнаты
        for (int i = 0; i < plan.rooms.Count; i++)
        {
            var room = plan.rooms[i];
            if (room.id < 0) room.id = i;

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
                if (_clearedRooms.Contains(room.id))
                {
                    Debug.Log($"[Dungeon] Комната {room.id} уже зачищена — содержимое не спавнится");
                    continue;
                }

                var spawned = roomSpawner.SpawnRoomObjects(roomComponent, room.data, gameConfig.floorSettings.mlModifiers);
                RegisterRoomEnemies(room.id, spawned);
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

    /// <summary>
    /// Регистрирует заспавненных врагов комнаты для трекинга зачистки.
    /// </summary>
    private void RegisterRoomEnemies(int roomId, List<GameObject> spawned)
    {
        if (spawned == null) return;

        foreach (var obj in spawned)
        {
            if (obj == null) continue;
            var npc = obj.GetComponentInChildren<NpcController>();
            if (npc == null) continue;

            if (!_roomEnemies.TryGetValue(roomId, out var set))
            {
                set = new HashSet<GameObject>();
                _roomEnemies[roomId] = set;
            }
            set.Add(npc.gameObject);
        }
    }

    /// <summary>
    /// Возвращает id комнаты, содержащей данную точку, или -1.
    /// </summary>
    private int FindRoomIdAt(Vector3 position)
    {
        if (_currentPlan == null) return -1;

        for (int i = 0; i < _currentPlan.rooms.Count; i++)
        {
            var room = _currentPlan.rooms[i];
            if (room?.data == null) continue;
            if (GetRoomBounds(room).Contains(position))
                return room.id;
        }
        return -1;
    }

    /// <summary>Мировые границы комнаты (та же математика, что в DungeonGenerator.GetWorldBounds).</summary>
    private Bounds GetRoomBounds(RoomInstance room)
    {
        Bounds localBounds = new Bounds(room.data.center, room.data.size);
        Vector3 min = localBounds.min;
        Vector3 max = localBounds.max;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z)
        };

        Bounds worldBounds = new Bounds(room.position + room.rotation * corners[0], Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
            worldBounds.Encapsulate(room.position + room.rotation * corners[i]);
        return worldBounds;
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
        if (player == null) return;

        var rb = player.gameObject.GetComponent<Rigidbody>();

        // Восстановление позиции из сохранённой экспедиции
        if (_restoredState != null)
        {
            player.transform.position = _restoredState.playerPosition;
            player.transform.rotation = Quaternion.Euler(_restoredState.playerRotationEuler);
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log($"[Dungeon] Игрок восстановлен в позиции {player.transform.position}");
            return;
        }

        var spawnPoint = plan.rooms[plan.startRoomIndex];

        if (rb != null)
        {
            var spawnPointsList = spawnPoint.spawnPoints.Where(point => point.spawnType == SpawnType.Player).ToList();
            var point = spawnPointsList[UnityEngine.Random.Range(0, spawnPointsList.Count - 1)];
            player.gameObject.transform.position = point.GetSpawnPosition();
            player.gameObject.transform.rotation = point.GetSpawnRotation();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //Debug.Log($"After {player.transform.position}");
    }

    // ===== ISaveModule (секция "expedition") =====

    public string Key => "expedition";

    public string CaptureStateJson()
    {
        if (_expeditionEnded || _currentPlan == null || _currentPlan.rooms.Count == 0)
            return null;

        var player = ServiceLocator.Instance.GetService<IPlayerMovementService>() as PlayerMovementService;

        var state = new ExpeditionState
        {
            isActive = true,
            floorIndex = settings?.floorIndex ?? 0,
            seed = settings?.seed ?? 0,
            startRoomIndex = _currentPlan.startRoomIndex,
            exitRoomIndex = _currentPlan.exitRoomIndex,
            playerRoomId = player != null ? FindRoomIdAt(player.transform.position) : -1,
            playerPosition = player != null ? player.transform.position : Vector3.zero,
            playerRotationEuler = player != null ? player.transform.eulerAngles : Vector3.zero,
        };

        for (int i = 0; i < _currentPlan.rooms.Count; i++)
        {
            var room = _currentPlan.rooms[i];
            if (room?.data == null || room.id < 0) continue;

            state.rooms.Add(new ExpeditionRoomState
            {
                roomId = room.id,
                prefabName = room.data.name,
                position = room.position,
                rotationEuler = room.rotation.eulerAngles,
                roomType = (int)room.data.roomType,
                cleared = _clearedRooms.Contains(room.id),
            });
        }

        return JsonUtility.ToJson(state);
    }

    public void RestoreStateJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        _restoredState = DeserializeState(json);
    }

    /// <summary>
    /// Завершает экспедицию поражением/досрочным выходом:
    /// секция "expedition" будет удалена из сохранения.
    /// </summary>
    public void EndExpedition()
    {
        if (_expeditionEnded) return;
        _expeditionEnded = true;
        SaveService?.DeleteSection(Key);
        SaveService?.RequestSave();
        Debug.Log("[Dungeon] Экспедиция завершена");
    }

    /// <summary>
    /// Завершает экспедицию победой (игрок достиг Exit-комнаты):
    /// инкрементирует статистику побед и удаляет секцию "expedition".
    /// </summary>
    public void CompleteExpedition()
    {
        if (_expeditionEnded) return;
        _expeditionEnded = true;

        var profile = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        profile?.UpdatePlayerStats(s => s.victories++);

        SaveService?.DeleteSection(Key);
        SaveService?.RequestSave();
        Debug.Log("[Dungeon] Экспедиция завершена победой");
    }

    // ===== События =====

    public void OnEnemyKilled(EnemyKilledEvent evt)
    {
        if (evt.Victim == null || _currentPlan == null) return;

        ServiceLocator.Instance.GetService<IPlayerProfileService>()
            ?.UpdatePlayerStats(s => s.enemiesKilled++);

        int roomId = FindRoomIdAt(evt.Victim.transform.position);
        if (roomId < 0) return;

        if (_roomEnemies.TryGetValue(roomId, out var enemies))
        {
            enemies.Remove(evt.Victim);
            if (enemies.Count == 0 && !_clearedRooms.Contains(roomId))
            {
                _clearedRooms.Add(roomId);
                SaveService?.RequestSave();
                Debug.Log($"[Dungeon] Комната {roomId} зачищена");
            }
        }
    }

    public void OnPlayerDied(PlayerDiedEvent evt)
    {
        // Политика: смерть в рейде завершает экспедицию
        // (персонаж будет перегенерирован архетипом при возврате в лагерь)
        ServiceLocator.Instance.GetService<IPlayerProfileService>()
            ?.UpdatePlayerStats(s => s.defeats++);
        EndExpedition();
    }

    protected override void OnDestroy()
    {
        EventBus.Unsubscribe(this);
        // Модуль выгружается: SaveService заморозит последнее состояние
        // экспедиции в "сиротскую" секцию (или удалит её, если экспедиция завершена).
        SaveService?.UnregisterModule(this);
        base.OnDestroy();
    }

    protected override System.Type GetServiceType() => typeof(IDungeonManagerService);
}
