using EventBusSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManagerService : BaseService, IDungeonManagerService, IGoldChangedEventSubscriber, IHealthChangedEventSubscriber
{

    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private int baseLootCount = 3;
    [SerializeField] private int baseTrapCount = 2;

    // Параметры, которые будет изменять ML-агент
    public float enemyMultiplier { get; set; } = 1f;
    public float lootMultiplier { get; set; } = 1f;
    public float trapMultiplier { get; set; } = 1f;

    private int _requiredGold;
    private bool _levelCompleted = false;
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
    [SerializeField] private int minLoot = 3;
    [SerializeField] private int minWalls = 2;
    [SerializeField] private int minSpikes = 2;

    [Header("Respawn")]
    [SerializeField] private float respawnCheckInterval = 5f;

    // Текущие количества
    private int _currentLoot;
    private int _currentWalls;
    private int _currentSpikes;

    // Свободные точки для каждого типа
    private List<SpawnPoint> _freeLootPoints;
    private List<SpawnPoint> _freeWallPoints;
    private List<SpawnPoint> _freeSpikePoints;
    private List<SpawnPoint> playerPoints;


    private void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        InitializeFreePoints();
        SpawnAllInitial();
        InvokeRepeating(nameof(CheckThresholdsAndRespawn), respawnCheckInterval, respawnCheckInterval);
    }

    public void OnEnable()
    {
        _requiredGold = Random.Range(50, 150);
        EventBus.Subscribe(this);
    }

    public void OnGoldChanged(GoldChangedEvent evt)
    {
        if (_levelCompleted) return;
        if (evt.NewGold >= _requiredGold)
        {
            _levelCompleted = true;

            var profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
            profile.CurrentProfile.stats.victories++;
            ServiceLocator.Instance.GetService<IPlayerProfileService>().SaveProfile(profile.CurrentProfile);

            EventBus.RaiseEvent<ILevelVictoryEventSubscriber>(s => s.OnLevelVictory());
            var inputService = ServiceLocator.Instance.GetService<IInputService>();
            inputService.DisableGameplayInput();

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

            EventBus.RaiseEvent<ILevelDefeatEventSubscriber>(s => s.OnLevelDefeat());
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

    //private void SpawnAll()
    //{
    //    var playerSpawnPoints = new List<SpawnPoint>();
    //    foreach (var point in allSpawnPoints)
    //    {
    //        switch (point.spawnType)
    //        {
    //            case SpawnType.Loot:
    //                SpawnRandom(lootPrefabs, point, baseLootCount);
    //                break;
    //            case SpawnType.Wall:
    //                SpawnRandom(wallPrefabs, point, baseTrapCount - Random.Range(0, baseTrapCount / 2));
    //                break;
    //            case SpawnType.Spikes:
    //                SpawnRandom(spikePrefabs, point, baseTrapCount - Random.Range(0, baseTrapCount));
    //                break;
    //            case SpawnType.Player:
    //                playerSpawnPoints.Add(point);
    //                break;
    //        }
    //    }
    //    SpawnPlayer(playerSpawnPoints);
    //}

    private void SpawnPlayer(List<SpawnPoint> points)
    {
        var player = (PlayerMovementService) ServiceLocator.Instance.GetService<IPlayerMovementService>();
        var spawnPoint = points[Random.Range(0, points.Count)];
        Debug.Log($"Before {player.transform.position}");

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

    //private void SpawnRandom(GameObject[] prefabs, ISpawnPoint point, int maxCount)
    //{
    //    if (prefabs.Length == 0) return;
    //    var prefab = prefabs[Random.Range(0, prefabs.Length)];
    //    var instance = Instantiate(prefab, point.GetSpawnPosition(), point.GetSpawnRotation());
    //    switch (point.spawnType) 
    //    {
    //        case SpawnType.Loot:
    //            instance.transform.parent = GameObject.Find("Entity").transform;
    //            break;
    //    }
    //}

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

        var despawnable = obj.GetComponent<Despawnable>();
        if (despawnable != null)
        {
            despawnable.SetSpawnPoint(point);
            despawnable.OnDespawned += HandleDespawned;
        }

        counter++;
        freePoints.Remove(point);
    }

    private void SpawnMultipleAtPoints(List<SpawnPoint> points, GameObject[] prefabs, SpawnType type, ref int counter, ref List<SpawnPoint> freePoints)
    {
        var pointsCopy = points.ToList();
        foreach (var point in pointsCopy)
        {
            SpawnSingleAtPoint(point, prefabs, type, ref counter, ref freePoints);
        }
    }

    private void SpawnAllInitial()
    {

        SpawnMultipleAtPoints(_freeLootPoints, lootPrefabs, SpawnType.Loot, ref _currentLoot, ref _freeLootPoints);
        SpawnMultipleAtPoints(_freeWallPoints, wallPrefabs, SpawnType.Wall, ref _currentWalls, ref _freeWallPoints);
        SpawnMultipleAtPoints(_freeSpikePoints, spikePrefabs, SpawnType.Spikes, ref _currentSpikes, ref _freeSpikePoints);
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
