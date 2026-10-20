using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ProgressionService : BaseService, IProgressionService
{
    [Header("Progression Data")]
    [SerializeField] private ProgressionCurveData progressionCurve;
    [SerializeField] private TraitDatabase traitDatabase;
    [SerializeField] private SkillDatabase skillDatabase;

    private IPlayerProfileService _profileService;
    private ICharacterStatsCalculator _characterStatsCalculator;
    private Dictionary<string, SkillNodeData> _allSkillNodes;
    private Dictionary<string, TraitData> _allTraits;
    public Dictionary<string, TraitData> AllTraits => _allTraits;
    public Dictionary<string, SkillNodeData> AllSkillNodes => _allSkillNodes;

    public int CurrentXP => _profileService?.CurrentProfile?.currentXP ?? 0;
    public int CurrentLevel => _profileService?.CurrentProfile?.currentLevel ?? 1;
    public int XPToNextLevel => CalculateXPToNextLevel();
    public int SkillPoints => _profileService?.CurrentProfile?.unspentSkillPoints ?? 0;
    public int MaxLevel => progressionCurve != null ? progressionCurve.maxLevel : 40;

    public event Action<int> OnLevelUp;
    public event Action<int> OnXPChanged;
    public event Action<int> OnSkillPointsChanged;
    public event Action<int> OnInsightChanged;
    public event Action<SkillNodeData> OnNodeUnlocked;
    public event Action<SkillNodeData> OnNodeLocked;
    public event Action<string> OnTraitAdded;
    public event Action<string> OnTraitRemoved;

    protected override Type GetServiceType() => typeof(IProgressionService);

    private void Start()
    {
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _characterStatsCalculator = ServiceLocator.Instance.GetService<ICharacterStatsCalculator>();
        LoadSkillNodes();
        LoadTraits();

        if (progressionCurve == null)
        {
            Debug.LogError("[ProgressionService] ProgressionCurveData не назначен!");
        }

        if (_profileService?.CurrentProfile != null)
        {
            InitializeProfileIfNeeded();
            ApplyAllActiveTraits();
            ApplyActiveSkills();
        }
    }

    private void LoadSkillNodes()
    {
        if(skillDatabase != null)
        {
            _allSkillNodes = skillDatabase.Database;
            Debug.Log($"[ProgressionService] Загружено {_allSkillNodes.Count} узлов навыков");
        }
        else
        {
            _allSkillNodes = new Dictionary<string, SkillNodeData>();
            Debug.LogWarning("[ProgressionService] SkillDatabase не назначен в Inspector");
        }

    }

    private void LoadTraits()
    {
        if (traitDatabase != null)
        {
            _allTraits = traitDatabase.Database;
            Debug.Log($"[ProgressionService] Загружено {_allTraits.Count} особенностей из TraitDatabase");
        }
        else
        {
            _allTraits = new Dictionary<string, TraitData>();
            Debug.LogWarning("[ProgressionService] TraitDatabase не назначен в Inspector");
        }
    }

    private void InitializeProfileIfNeeded()
    {
        var profile = _profileService.CurrentProfile;

        if (profile.currentLevel <= 0)
            profile.currentLevel = 1;

        if (profile.currentXP < 0)
            profile.currentXP = 0;

        if (profile.unspentSkillPoints < 0)
            profile.unspentSkillPoints = 0;
    }

    private void ApplyAllActiveTraits()
    {
        var profile = _profileService.CurrentProfile;
        if (profile.activeTraitIds == null) return;

        foreach (var traitId in profile.activeTraitIds)
        {
            if (_allTraits.TryGetValue(traitId, out var trait))
            {
                ApplyTraitEffects(traitId);
            }
        }

        _characterStatsCalculator?.SyncCurrentHealthWithMax();

        Debug.Log($"[ProgressionService] Применены эффекты {profile.activeTraitIds.Count} активных особенностей");
    }

    private void ApplyActiveSkills()
    {
        var profile = _profileService.CurrentProfile;
        if(profile.unlockedSkillIds == null) return;

        foreach(var skillNodeId in profile.unlockedSkillIds)
        {
            if (_allSkillNodes.TryGetValue(skillNodeId, out var skillNode))
                ApplySkillEffect(skillNodeId);
        }

        _characterStatsCalculator?.SyncCurrentHealthWithMax();

        Debug.Log($"[ProgressionService] Применены эффекты {profile.unlockedSkillIds.Count} активных способностей");
    }

    public void AddXP(int amount, XPSource source)
    {
        if (amount <= 0) return;

        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        profile.currentXP += amount;
        OnXPChanged?.Invoke(profile.currentXP);
        _profileService.SaveProfile(profile);
        CheckLevelUp(profile);
    }

    private void CheckLevelUp(PlayerProfile profile)
    {
        if (progressionCurve == null) return;

        int maxLevel = progressionCurve.maxLevel;

        while (profile.currentLevel < maxLevel)
        {
            int xpForNext = progressionCurve.GetXPForLevel(profile.currentLevel + 1);

            if (xpForNext < 0)
            {
                Debug.LogWarning($"[ProgressionService] Некорректные данные для уровня {profile.currentLevel + 1}");
                break;
            }

            if (profile.currentXP >= xpForNext)
            {
                profile.currentLevel++;
                int pointsToGrant = progressionCurve.skillPointsPerLevel;
                profile.unspentSkillPoints += pointsToGrant;

                Debug.Log($"[ProgressionService] Уровень повышен до {profile.currentLevel}! Получено {pointsToGrant} очков навыков.");
                OnLevelUp?.Invoke(profile.currentLevel);
                OnSkillPointsChanged?.Invoke(profile.unspentSkillPoints);
                _profileService.SaveProfile(profile);
            }
            else
            {
                break;
            }
        }

        if (profile.currentLevel >= maxLevel)
        {
            Debug.Log($"[ProgressionService] Достигнут максимальный уровень {maxLevel}");
        }
    }

    private int CalculateXPToNextLevel()
    {
        if (progressionCurve == null) return 0;

        int currentLevel = CurrentLevel;
        int maxLevel = progressionCurve.maxLevel;

        if (currentLevel >= maxLevel)
            return -1;

        int xpRequired = progressionCurve.GetXPForLevel(currentLevel + 1);
        return xpRequired >= 0 ? xpRequired : 0;
    }

    public void GrantSkillPoints(int amount)
    {
        if (amount <= 0) return;

        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        profile.unspentSkillPoints += amount;
        OnSkillPointsChanged?.Invoke(profile.unspentSkillPoints);
        _profileService.SaveProfile(profile);
        Debug.Log($"[ProgressionService] Выдано {amount} очков навыков. Всего: {profile.unspentSkillPoints}");
    }

    public bool TrySpendSkillPoints(int amount)
    {
        if (amount <= 0) return false;

        var profile = _profileService?.CurrentProfile;
        if (profile == null) return false;

        if (profile.unspentSkillPoints >= amount)
        {
            profile.unspentSkillPoints -= amount;
            OnSkillPointsChanged?.Invoke(profile.unspentSkillPoints);
            _profileService.SaveProfile(profile);
            return true;
        }

        Debug.LogWarning($"[ProgressionService] Недостаточно очков навыков. Требуется: {amount}, Доступно: {profile.unspentSkillPoints}");
        return false;
    }

    public void ResetToLevel(int targetLevel)
    {
        if (progressionCurve == null)
        {
            Debug.LogError("[ProgressionService] ProgressionCurveData не назначен!");
            return;
        }

        int maxLevel = progressionCurve.maxLevel;

        if (targetLevel < 1)
        {
            Debug.LogWarning("[ProgressionService] Уровень не может быть меньше 1");
            targetLevel = 1;
        }

        if (targetLevel > maxLevel)
        {
            Debug.LogWarning($"[ProgressionService] Уровень не может превышать максимальный ({maxLevel})");
            targetLevel = maxLevel;
        }

        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        profile.currentLevel = targetLevel;

        if (targetLevel == 1)
        {
            profile.currentXP = 0;
        }
        else
        {
            int xpForTargetLevel = progressionCurve.GetXPForLevel(targetLevel);
            if (xpForTargetLevel >= 0)
            {
                profile.currentXP = xpForTargetLevel;
            }
            else
            {
                Debug.LogWarning($"[ProgressionService] Некорректные данные для уровня {targetLevel}");
                profile.currentXP = 0;
            }
        }

        profile.unspentSkillPoints = (targetLevel - 1) * progressionCurve.skillPointsPerLevel;

        OnXPChanged?.Invoke(profile.currentXP);
        OnLevelUp?.Invoke(profile.currentLevel);
        OnSkillPointsChanged?.Invoke(profile.unspentSkillPoints);

        Debug.Log($"[ProgressionService] Прогресс сброшен на уровень {targetLevel}. XP: {profile.currentXP}, Очки навыков: {profile.unspentSkillPoints}");
    }

    public bool CanPerformRitual(string ritualId)
    {
        return false;
    }

    public bool TryPerformRitual(string ritualId)
    {
        Debug.LogWarning($"[ProgressionService] Ритуал {ritualId} не реализован");
        return false;
    }

    public IReadOnlyCollection<SkillNodeData> GetAllNodes()
    {
        return _allSkillNodes.Values;
    }

    /// <summary>
    /// Получить все доступные для открытия ноды способностей
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<SkillNodeData> GetAvailableNodes()
    {
        var available = new List<SkillNodeData>();
        var profile = _profileService?.CurrentProfile;
        if (profile == null) return available;

        foreach (var node in _allSkillNodes.Values)
        {
            if (IsUnlocked(node.nodeId)) continue;

            if (CanUnlock(node.nodeId, out var reason) && reason == UnlockReason.Ok)
            {
                available.Add(node);
            }
        }
        return available;
    }

    public IReadOnlyCollection<SkillNodeData> GetUnlockedNodes()
    {
        var unlocked = new List<SkillNodeData>();
        var profile = _profileService?.CurrentProfile;
        if (profile == null) return unlocked;

        foreach (var nodeId in profile.unlockedSkillIds)
        {
            if (_allSkillNodes.TryGetValue(nodeId, out var node))
            {
                unlocked.Add(node);
            }
        }
        return unlocked;
    }

    public void CheckLevelUp()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile != null)
        {
            CheckLevelUp(profile);
        }
    }

    public bool IsUnlocked(string nodeId)
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null) return false;
        return profile.unlockedSkillIds.Contains(nodeId);
    }

    public bool CanUnlock(string nodeId, out UnlockReason reason)
    {
        reason = UnlockReason.Ok;

        if (!_allSkillNodes.TryGetValue(nodeId, out var node))
        {
            reason = UnlockReason.RequirementFailed;
            return false;
        }

        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            reason = UnlockReason.RequirementFailed;
            return false;
        }

        if (SkillPoints < node.cost)
        {
            reason = UnlockReason.NoPoints;
            return false;
        }

        if (profile.currentLevel < node.minInsightLevel)
        {
            reason = UnlockReason.RequirementFailed;
            return false;
        }

        foreach (var requiredId in node.requiredNodes)
        {
            if (!IsUnlocked(requiredId))
            {
                reason = UnlockReason.MissingPrerequisites;
                return false;
            }
        }

        foreach (var requiredTrait in node.requiredTraits)
        {
            if (!profile.activeTraitIds.Contains(requiredTrait))
            {
                reason = UnlockReason.RequirementFailed;
                return false;
            }
        }

        return true;
    }

    public bool TryUnlock(string nodeId)
    {
        if (!_allSkillNodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogWarning($"[ProgressionService] Узел {nodeId} не найден");
            return false;
        }

        if (IsUnlocked(nodeId))
        {
            Debug.LogWarning($"[ProgressionService] Узел {nodeId} уже разблокирован");
            return false;
        }

        if (!CanUnlock(nodeId, out var reason))
        {
            Debug.LogWarning($"[ProgressionService] Невозможно разблокировать {nodeId}: {reason}");
            return false;
        }

        var profile = _profileService?.CurrentProfile;
        if (profile == null) return false;

        if (!TrySpendSkillPoints(node.cost))
        {
            return false;
        }

        AddSkill(nodeId);
        return true;
    }

    public void AddTrait(string traitId)
    {
        if (!_allTraits.TryGetValue(traitId, out var trait))
        {
            Debug.LogWarning($"[ProgressionService] Особенность {traitId} не найдена в базе данных");
            return;
        }

        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        if (profile.activeTraitIds.Contains(traitId))
        {
            Debug.LogWarning($"[ProgressionService] Особенность {traitId} уже активна");
            return;
        }

        profile.activeTraitIds.Add(traitId);
        ApplyTraitEffects(trait.traitId);
        _profileService.SaveProfile(profile);
        OnTraitAdded?.Invoke(traitId);
        Debug.Log($"[ProgressionService] Особенность {trait.displayName} ({traitId}) добавлена");
    }

    public void RemoveTrait(string traitId)
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        if (!profile.activeTraitIds.Contains(traitId))
        {
            Debug.LogWarning($"[ProgressionService] Особенность {traitId} не активна");
            return;
        }

        RemoveTraitEffects(traitId);
        profile.activeTraitIds.Remove(traitId);
        _profileService.SaveProfile(profile);
        OnTraitRemoved?.Invoke(traitId);
        Debug.Log($"[ProgressionService] Особенность {traitId} удалена");
    }

    private void ApplyTraitEffects(string traitId)
    {
        if (!_allTraits.TryGetValue(traitId, out var trait)) return;

        if (_characterStatsCalculator == null)
        {
            Debug.LogWarning("[ProgressionService] CharacterStatsCalculator не найден");
            return;
        }

        foreach (var effect in trait.effects)
        {
            if (effect != null)
            {
                effect.Apply(_characterStatsCalculator);
            }
        }
    }

    private void RemoveTraitEffects(string traitId)
    {
        if (!_allTraits.TryGetValue(traitId, out var trait)) return;

        if (_characterStatsCalculator == null) return;

        foreach (var effect in trait.effects)
        {
            if (effect != null)
            {
                effect.Remove(_characterStatsCalculator);
            }
        }
    }

    public void AddSkill(string skillId)
    {
        _allSkillNodes.TryGetValue(skillId, out var node);
        if (node == null) return;

        var profile = _profileService?.CurrentProfile;
        profile.unlockedSkillIds.Add(skillId);

        foreach (var effect in node.effects)
        {
            if (_characterStatsCalculator != null)
            {
                effect.Apply(_characterStatsCalculator);
            }
        }
        OnNodeUnlocked?.Invoke(node);
        Debug.Log($"[ProgressionService] Узел {skillId} ({node.displayName}) разблокирован");
    }

    public void RemoveSkill(string skillId)
    {
        var profile = _profileService?.CurrentProfile;
        if(!_allSkillNodes.TryGetValue(skillId, out var node))
        {
            Debug.LogWarning($"[ProgressionService] Способность {skillId} не найдена, нечего удалять");
            return;
        }
        if (profile == null)
        {
            Debug.LogWarning("[ProgressionService] Профиль не найден");
            return;
        }

        if (!profile.unlockedSkillIds.Contains(skillId))
        {
            Debug.LogWarning($"[ProgressionService] Способность {skillId} не активна");
            return;
        }
        profile.unlockedSkillIds.Remove(skillId);
        OnNodeLocked.Invoke(node);
        Debug.Log($"[ProgressionService] Узел {skillId} ({node.displayName}) заблокирован");
    }

    private void ApplySkillEffect(string skillId)
    {
        if (!_allSkillNodes.TryGetValue(skillId, out var skill)) return;

        if (_characterStatsCalculator == null)
        {
            Debug.LogWarning("[ProgressionService] CharacterStatsCalculator не найден");
            return;
        }

        foreach (var effect in skill.effects)
        {
            if (effect != null)
            {
                effect.Apply(_characterStatsCalculator);
            }
        }
    }

    public IReadOnlyList<TraitData> GetActiveTraits()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null) return new List<TraitData>();

        var activeTraits = new List<TraitData>();
        foreach (var traitId in profile.activeTraitIds)
        {
            if (_allTraits.TryGetValue(traitId, out var trait))
            {
                activeTraits.Add(trait);
            }
        }
        return activeTraits;
    }
}
