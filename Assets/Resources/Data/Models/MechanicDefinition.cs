using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Mechanic Definition")]
public class MechanicDefinition : ScriptableObject
{
    public string mechanicId;
    public string displayName;
    public Sprite icon;

    [Header("Activation")]
    public string requiredSkillNodeId;
    public string requiredArchetypeId;

    [Header("Limits")]
    public int minValue = 0;
    public int maxValue = 100;

    [Header("Tiers")]
    [Tooltip("Пороги с кумулятивными бонусами")]
    public List<MechanicTier> tiers;

    [Header("Gain/Loss")]
    [Tooltip("Правила повышения шкалы")]
    public List<MechanicGainRule> gainRules;
    [Tooltip("Правила понижения шкалы")]
    public List<MechanicLossRule> lossRules;
}

[System.Serializable]
public class MechanicTier
{
    [Tooltip("Градация показателей шкалы")]
    public int threshold;
    [Tooltip("Название градаций")]
    public string tierName;
    public List<SkillEffectData> effects;
}

public enum MechanicGainRule
{

}

public enum MechanicLossRule
{

}