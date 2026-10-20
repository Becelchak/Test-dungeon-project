using System;
using System.Collections.Generic;


public enum XPSource {
    Combat, 
    Quest, 
    Exploration, 
    Dialogue, 
    Ritual,
    ItemUse,
    Console
}

public enum UnlockReason
{
    Console,
    Level,
    Node,
    Ok,
    RequirementFailed,
    NoPoints,
    MissingPrerequisites
}
public interface IProgressionService
{
    // Состояние
    int CurrentXP { get; }
    int CurrentLevel { get; }
    int SkillPoints { get; }

    int XPToNextLevel { get; }
    int MaxLevel { get; }
    Dictionary<string, TraitData> AllTraits {  get; }
    Dictionary<string, SkillNodeData> AllSkillNodes { get; }

    // Действия
    void AddXP(int amount, XPSource source);
    void GrantSkillPoints(int amount);
    bool TrySpendSkillPoints(int amount);
    void ResetToLevel(int targetLevel);

    /// <summary>
    /// Возможность сотворить конкретный ритуал
    /// </summary>
    /// <param name="ritualId">Id ритуала</param>
    /// <returns></returns>
    bool CanPerformRitual(string ritualId);
    bool TryPerformRitual(string ritualId);
    IReadOnlyCollection<SkillNodeData> GetAllNodes();
    IReadOnlyCollection<SkillNodeData> GetAvailableNodes();
    IReadOnlyCollection<SkillNodeData> GetUnlockedNodes();
    void CheckLevelUp();
    bool IsUnlocked(string nodeId);
    bool CanUnlock(string nodeId, out UnlockReason reason);
    bool TryUnlock(string nodeId);
    void AddTrait(string traitId);
    void RemoveTrait(string traitId);
    void AddSkill(string skillId);
    void RemoveSkill(string skillId);
    IReadOnlyList<TraitData> GetActiveTraits();


    // События
    event Action<int> OnLevelUp;
    event Action<int> OnXPChanged;
    event Action<int> OnSkillPointsChanged;
    /// <summary>
    /// Снижение через ритуал
    /// </summary>
    event Action<int> OnInsightChanged;
    event Action<SkillNodeData> OnNodeUnlocked;
    public event Action<SkillNodeData> OnNodeLocked;
}