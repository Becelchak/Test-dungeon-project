using UnityEngine;
using Unity.MLAgents.Policies;
using EventBusSystem;
using Unity.InferenceEngine;

public class MLDungeonController : MonoBehaviour, IEmotionsUpdatedSubscriber
{
    [Header("ML Agent")]
    [SerializeField] private MLInferenceAgent inferenceAgent;
    [SerializeField] private ModelAsset model;
    [SerializeField] private InferenceDevice inferenceDevice = InferenceDevice.Default;

    [Header("Action Settings")]
    [SerializeField] private float actionScale = 0.1f;
    [SerializeField] private float minMultiplier = 0.5f;
    [SerializeField] private float maxMultiplier = 5f;

    private DungeonManagerService _dungeonManager;
    private PlayerProfileService _playerProfile;
    private bool _hasApplied = false;

    private void Awake()
    {
        _dungeonManager = ServiceLocator.Instance.GetService<IDungeonManagerService>() as DungeonManagerService;
        _playerProfile = ServiceLocator.Instance.GetService<IPlayerProfileService>() as PlayerProfileService;

        if (inferenceAgent == null || _dungeonManager == null || _playerProfile == null)
        {
            Debug.LogError("Не удалось найти необходимые компоненты");
            return;
        }

        var behaviorParams = inferenceAgent.GetComponent<BehaviorParameters>();
        if (behaviorParams != null)
        {
            behaviorParams.BehaviorType = BehaviorType.InferenceOnly;
            behaviorParams.Model = model;
            behaviorParams.InferenceDevice = inferenceDevice;
        }


        float[] emotions = _playerProfile.CurrentProfile.LastEmotions;
        if (emotions == null || emotions.Length != 8)
        {
            Debug.LogWarning("Нет эмоций в профиле, используем нейтральные");
            emotions = new float[8] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
        }
        ApplyEmotions(emotions);
    }

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        if (inferenceAgent != null)
            inferenceAgent.OnActionsReceived += ApplyActions;
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
        if (inferenceAgent != null)
            inferenceAgent.OnActionsReceived -= ApplyActions;
    }

    private void OnDestroy()
    {
        if (inferenceAgent != null)
            inferenceAgent.OnActionsReceived -= ApplyActions;
    }

    private void ApplyEmotions(float[] emotions)
    {

        Debug.Log($"ПРОЧИТАЛ ЭМОЦИИ {emotions}");
        // Обновляем наблюдения
        inferenceAgent.currentEmotions = emotions;
        inferenceAgent.wallMultiplier = _dungeonManager.wallMultiplier;
        inferenceAgent.lootMultiplier = _dungeonManager.lootMultiplier;
        inferenceAgent.trapMultiplier = _dungeonManager.trapMultiplier;
        inferenceAgent.goldMultiplier = _dungeonManager.goldMultiplier;
        inferenceAgent.respawnMultiplier = _dungeonManager.respawnMultiplier;
        inferenceAgent.normalizedHealth = (float)_playerProfile.CurrentProfile.health / _playerProfile.CurrentProfile.maxHealth;
        inferenceAgent.normalizedGold = _playerProfile.CurrentProfile.goldCount / (float)_dungeonManager.RequiredGold;
        inferenceAgent.normalizedProgress = 0f;

        // Запрашиваем решение
        inferenceAgent.RequestDecision();
    }

    // Если нужно реагировать на эмоции в реальном времени (например, если диалог идёт прямо во время нахождения в подземелье)
    public void OnEmotionsUpdated(EmotionsUpdatedEvent evt)
    {
        ApplyEmotions(evt.Emotions);
    }

    private void ApplyActions(float[] actions)
    {
        if (_hasApplied) return;
        _hasApplied = true;

        if (actions.Length < 5) return;

        ApplyAction(ref _dungeonManager.wallMultiplier, actions[0]);
        ApplyAction(ref _dungeonManager.lootMultiplier, actions[1]);
        ApplyAction(ref _dungeonManager.trapMultiplier, actions[2]);
        ApplyAction(ref _dungeonManager.goldMultiplier, actions[3]);
        ApplyAction(ref _dungeonManager.respawnMultiplier, actions[4]);

        if (!_dungeonManager._isInitialized)
        {
            _dungeonManager._isInitialized = true;
            _dungeonManager.InitializeDungeon();
        }

        if (inferenceAgent != null)
            inferenceAgent.OnActionsReceived -= ApplyActions;

        Debug.Log($"НОВЫЕ МНОЖИТЕЛИ: w{_dungeonManager.wallMultiplier}, l{_dungeonManager.lootMultiplier}, t{_dungeonManager.trapMultiplier}, g{_dungeonManager.goldMultiplier}");
    }

    private void ApplyAction(ref float multiplier, float delta)
    {
        multiplier *= (1f + delta * actionScale);
        multiplier = Mathf.Clamp(multiplier, minMultiplier, maxMultiplier);
    }
}