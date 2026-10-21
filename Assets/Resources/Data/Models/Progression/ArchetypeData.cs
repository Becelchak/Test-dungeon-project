using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Archetype")]
public class ArchetypeData : ScriptableObject
{
    public string archetypeId;
    public string displayName;
    public Sprite portrait;

    [Header("Stat Ranges (random within range)")]
    public MinMaxInt insightRange;
    public MinMaxInt strengthRange;
    public MinMaxInt intelligenceRange;
    public MinMaxInt agilityRange;
    public CharacterClassData classData;

    [Header("Skill Trees")]
    [Tooltip("Уникальная ветка типажа")]
    public SkillBranchData ownBranch;
    [Tooltip("Общие ветки типажей")]
    public List<SkillBranchData> allowedSharedBranches;
}
