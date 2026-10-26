using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Skill Node")]
public class SkillNodeData : ScriptableObject
{
    public string nodeId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Graph")]
    [Tooltip("Id предшественника навыка")]
    public List<string> requiredNodes;
    [Tooltip("Цена навыка")]
    public int cost = 1;

    [Header("Branch")]
    [Tooltip("К какому древу навыков принадлежит")]
    public SkillBranch branch;

    [Header("Effects")]
    [Tooltip("Какие полиморфные эффекты применяет")]
    public List<SkillEffectData> effects;

    [Header("Requirements (опционально)")]
    [Tooltip("Минимальный уровень прозрения игрока, для открытия навыка")]
    public int minInsightLevel = 1;
    public List<string> requiredTraits;
}