using EventBusSystem;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManagerService : BaseService, IDungeonManagerService, IGoldChangedEventSubscriber, IHealthChangedEventSubscriber
{

    //[SerializeField] private int baseEnemyCount = 5;
    //[SerializeField] private int baseLootCount = 3;
    //[SerializeField] private int baseTrapCount = 2;
    [SerializeField] private float _baseRespawnInterval;
    [SerializeField] private float baseRespawnIntervalMin = 18f;
    [SerializeField] private float baseRespawnIntervalMax = 21f;

    // Параметры, которые будет изменять ML-агент
    [SerializeField] public float wallMultiplier = 1f;
    [SerializeField] public float lootMultiplier = 1f;
    [SerializeField] public float trapMultiplier = 1f;
    [SerializeField] public float respawnMultiplier = 1f;
    [SerializeField] public float goldMultiplier = 1f;

    private int _requiredGold;
    private bool _levelCompleted = false;
    private bool _respawnEnabled = true;

    private List<GameObject> _spawnedObjects = new List<GameObject>();
    public int RequiredGold => _requiredGold;
    public bool IsLevelCompleted => _levelCompleted;

    [Header("Spawn Points")]
    [SerializeField] private GameObject spawnPointsObject;
    [SerializeField] private List<SpawnPoint> allSpawnPoints;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] lootPrefabs;
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject[] spikePrefabs;

    [Header("Thresholds")]
    [SerializeField] private int minLoot = 1;
    [SerializeField] private int minWalls = 1;
    [SerializeField] private int minSpikes = 1;

    [Header("Respawn")]
    [SerializeField] private float respawnCheckInterval = 5f;
    private float _currentRespawnInterval;

    // Текущие количества
    private int _currentLoot;
    private int _currentWalls;
    private int _currentSpikes;

    // Свободные точки для каждого типа
    private List<SpawnPoint> _freeLootPoints;
    private List<SpawnPoint> _freeWallPoints;
    private List<SpawnPoint> _freeSpikePoints;
    private List<SpawnPoint> playerPoints;

    // Флаги
    public bool _isInitialized = false;

    private void Awake()
    {
        base.Awake();
    }

    public void InitializeDungeon()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        SpawnAllInitial();
        //InvokeRepeating(nameof(CheckThresholdsAndRespawn), respawnCheckInterval, respawnCheckInterval);
    }

    private void Start()
    {
        InitializeFreePoints();
        //SpawnAllInitial();
        GenerateBaseRespawnInterval();
        StartRespawnTimer();
        InvokeRepeating(nameof(CheckThresholdsAndRespawn), respawnCheckInterval, respawnCheckInterval);
    }

    private void ResetFreePoints()
    {
        _freeLootPoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Loot).ToList();
        _freeWallPoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Wall).ToList();
        _freeSpikePoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Spikes).ToList();
    }

    public void OnEnable()
    {
        _requiredGold = Mathf.RoundToInt(Random.Range(50 * goldMultiplier, 100 * goldMultiplier));
        EventBusSystem.EventBus.Subscribe(this);
    }

    private void GenerateBaseRespawnInterval()
    {
        _baseRespawnInterval = Random.Range(baseRespawnIntervalMin, baseRespawnIntervalMax);
        UpdateCurrentRespawnInterval();

        // Уведомление о стартовом интервале
        EventBusSystem.EventBus.RaiseEvent<IRespawnIntervalChangedEventSubscriber>(
            s => s.OnShowNotification(new RespawnIntervalChangedEvent($"Интервал обновления: {_currentRespawnInterval:F1} сек", 2f))
        );
    }

    private void UpdateCurrentRespawnInterval()
    {
        _currentRespawnInterval = _baseRespawnInterval * respawnMultiplier;
    }

    private void StartRespawnTimer()
    {
        CancelInvoke(nameof(RespawnAll));
        InvokeRepeating(nameof(RespawnAll), _currentRespawnInterval, _currentRespawnInterval);
    }

    // Публичный метод для изменения множителя (вызывается ML-агентом)
    public void SetRespawnMultiplier(float multiplier)
    {
        respawnMultiplier = Mathf.Max(0.1f, multiplier);
        UpdateCurrentRespawnInterval();
        StartRespawnTimer(); // перезапуск с новым интервалом

        EventBusSystem.EventBus.RaiseEvent<IRespawnIntervalChangedEventSubscriber>(
            s => s.OnShowNotification(new RespawnIntervalChangedEvent($"Интервал изменён на {_currentRespawnInterval:F1} сек", 2f))
        );
    }

    private void RespawnAll()
    {
        if (!_respawnEnabled) return;

        // Уничтожаем все ранее заспавненные объекты
        foreach (var obj in _spawnedObjects)
        {

            if (obj != null) Destroy(obj);
        }
        _spawnedObjects.Clear();

        // Сбрасываем свободные точки (теперь все доступны)
        ResetFreePoints();

        // Заново спавним объекты
        SpawnAllRandom();

        EventBusSystem.EventBus.RaiseEvent<IRespawnIntervalChangedEventSubscriber>(
            s => s.OnShowNotification(new RespawnIntervalChangedEvent("Подземелье обновлено!", 1.5f))
        );
    }

    public void OnGoldChanged(GoldChangedEvent evt)
    {
        if (_levelCompleted) return;
        if (evt.NewGold >= _requiredGold)
        {
            _levelCompleted = true;

            var profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
            profile.CurrentProfile.stats.victories++;
            profile.CurrentProfile.stats.goldCollected += profile.CurrentProfile.goldCount;
            profile.CurrentProfile.goldCount = 0;
            ServiceLocator.Instance.GetService<IPlayerProfileService>().SaveProfile(profile.CurrentProfile);

            profile.CurrentProfile.health = profile.CurrentProfile.maxHealth;
            profile.SaveProfile(profile.CurrentProfile);

            EventBusSystem.EventBus.RaiseEvent<ILevelVictoryEventSubscriber>(s => s.OnLevelVictory());
            var inputService = ServiceLocator.Instance.GetService<IInputService>();
            inputService.DisableGameplayInput();
            var playerMove = ServiceLocator.Instance.GetService<IPlayerMovementService>();
            playerMove.StopMovement();

            Invoke(nameof(ReturnToMainScene), 2f);
        }
    }

    public void OnHealthChanged(HealthChangedEvent evt)
    {
        if (_levelCompleted) return;
        if (evt.CurrentHealth <= 0)
        {
            _levelCompleted = true;
            // Обновляем статистику поражений
            var profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
            profile.CurrentProfile.stats.defeats++;
            ServiceLocator.Instance.GetService<IPlayerProfileService>().SaveProfile(profile.CurrentProfile);

            profile.CurrentProfile.health = profile.CurrentProfile.maxHealth;
            profile.SaveProfile(profile.CurrentProfile);

            EventBusSystem.EventBus.RaiseEvent<ILevelDefeatEventSubscriber>(s => s.OnLevelDefeat());
            var inputService = ServiceLocator.Instance.GetService<IInputService>();
            inputService.DisableGameplayInput();

            Invoke(nameof(ReturnToMainScene), 2f);
        }
    }

    private void ReturnToMainScene()
    {
        SceneManager.LoadScene("DialogScene");
    }


    private void InitializeFreePoints()
    {
        allSpawnPoints = spawnPointsObject.GetComponentsInChildren<SpawnPoint>().ToList();
        // Разделяем все точки по типам
        _freeLootPoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Loot).ToList();
        _freeWallPoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Wall).ToList();
        _freeSpikePoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Spikes).ToList();
        playerPoints = allSpawnPoints.Where(p => p.spawnType == SpawnType.Player).ToList();
        SpawnPlayer(playerPoints);
    }

    private void SpawnPlayer(List<SpawnPoint> points)
    {
        var player = (PlayerMovementService) ServiceLocator.Instance.GetService<IPlayerMovementService>();
        var spawnPoint = points[Random.Range(0, points.Count)];

        var rb = player.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = spawnPoint.GetSpawnPosition();
            rb.rotation = spawnPoint.GetSpawnRotation();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            player.gameObject.transform.position = spawnPoint.GetSpawnPosition();
            player.gameObject.transform.rotation = spawnPoint.GetSpawnRotation();
        }
        Debug.Log($"After {player.transform.position}");
    }

    private void SpawnObjectsOfType(SpawnType type, int count)
    {
        List<SpawnPoint> freePoints;
        GameObject[] prefabs;
        int counter;

        switch (type)
        {
            case SpawnType.Loot:
                freePoints = _freeLootPoints;
                prefabs = lootPrefabs;
                counter = _currentLoot;
                break;
            case SpawnType.Wall:
                freePoints = _freeWallPoints;
                prefabs = wallPrefabs;
                counter = _currentWalls;
                break;
            case SpawnType.Spikes:
                freePoints = _freeSpikePoints;
                prefabs = spikePrefabs;
                counter = _currentSpikes;
                break;
            default: return;
        }

        int available = freePoints.Count;
        int toSpawn = Mathf.Min(count, available);

        for (int i = 0; i < toSpawn; i++)
        {
            int index = Random.Range(0, freePoints.Count);
            var point = freePoints[index];
            SpawnSingleAtPoint(point, prefabs, type, ref counter, ref freePoints);
        }
    }

    private void CheckThresholdsAndRespawn()
    {
        if (_currentLoot < minLoot)
        {
            int deficit = minLoot - _currentLoot;
            SpawnObjectsOfType(SpawnType.Loot, deficit);
        }
        if (_currentWalls < minWalls)
        {
            int deficit = minWalls - _currentWalls;
            SpawnObjectsOfType(SpawnType.Wall, deficit);
        }
        if (_currentSpikes < minSpikes)
        {
            int deficit = minSpikes - _currentSpikes;
            SpawnObjectsOfType(SpawnType.Spikes, deficit);
        }
    }

    public void FreeSpawnPoint(SpawnPoint point, SpawnType type)
    {
        switch (type)
        {
            case SpawnType.Loot:
                if (!_freeLootPoints.Contains(point))
                    _freeLootPoints.Add(point);
                break;
            case SpawnType.Wall:
                if (!_freeWallPoints.Contains(point))
                    _freeWallPoints.Add(point);
                break;
            case SpawnType.Spikes:
                if (!_freeSpikePoints.Contains(point))
                    _freeSpikePoints.Add(point);
                break;
        }
    }

    private void SpawnSingleAtPoint(SpawnPoint point, GameObject[] prefabs, SpawnType type, ref int counter, ref List<SpawnPoint> freePoints)
    {
        if (prefabs.Length == 0) return;

        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        var obj = Instantiate(prefab, point.GetSpawnPosition(), point.GetSpawnRotation());
        _spawnedObjects.Add(obj);

        var despawnable = obj.GetComponent<Despawnable>();
        if (despawnable != null)
        {
            despawnable.SetSpawnPoint(point);
            despawnable.OnDespawned += HandleDespawned;
        }

        counter++;
        freePoints.Remove(point);
    }

    private void SpawnMultipleAtPoints(List<SpawnPoint> points, GameObject[] prefabs, SpawnType type, ref int counter, ref List<SpawnPoint> freePoints, bool isRandom)
    {
        var pointsCopy = points.ToList();
        if (isRandom)
        {
            int minCountForType = 0;
            switch (type)
            {
                case SpawnType.Loot:
                    minCountForType = Mathf.Min(Mathf.RoundToInt(minLoot * lootMultiplier), freePoints.Count);
                    break;
                case SpawnType.Wall:
                    minCountForType = Mathf.Min(
                        Mathf.CeilToInt(minWalls * wallMultiplier), 
                        freePoints.Count);
                    break;
                case SpawnType.Spikes:
                    minCountForType = Mathf.Min(
                        Mathf.CeilToInt(minSpikes * trapMultiplier), 
                        freePoints.Count);
                    break;
            }
            for(var i = 0; i < minCountForType; i++)
            {
                var rndPoint = freePoints[Random.Range(0, freePoints.Count)];
                SpawnSingleAtPoint(rndPoint, prefabs, type, ref counter, ref freePoints);
            }
        }
        else
        {
            foreach (var point in pointsCopy)
            {
                SpawnSingleAtPoint(point, prefabs, type, ref counter, ref freePoints);
            }
        }
    }

    private void SpawnAllInitial()
    {

        SpawnMultipleAtPoints(_freeLootPoints, lootPrefabs, SpawnType.Loot, ref _currentLoot, ref _freeLootPoints, true);
        SpawnMultipleAtPoints(_freeWallPoints, wallPrefabs, SpawnType.Wall, ref _currentWalls, ref _freeWallPoints, true);
        SpawnMultipleAtPoints(_freeSpikePoints, spikePrefabs, SpawnType.Spikes, ref _currentSpikes, ref _freeSpikePoints, true);
    }

    private void SpawnAllRandom()
    {

        SpawnMultipleAtPoints(_freeLootPoints, lootPrefabs, SpawnType.Loot, ref _currentLoot, ref _freeLootPoints, true);
        SpawnMultipleAtPoints(_freeWallPoints, wallPrefabs, SpawnType.Wall, ref _currentWalls, ref _freeWallPoints, true);
        SpawnMultipleAtPoints(_freeSpikePoints, spikePrefabs, SpawnType.Spikes, ref _currentSpikes, ref _freeSpikePoints, true);
    }

    private void HandleDespawned(SpawnPoint point, SpawnType type)
    {

        switch (type)
        {
            case SpawnType.Loot: _currentLoot--; break;
            case SpawnType.Wall: _currentWalls--; break;
            case SpawnType.Spikes: _currentSpikes--; break;
        }

        FreeSpawnPoint(point, type);
    }


    protected override System.Type GetServiceType() => typeof(IDungeonManagerService);
}
