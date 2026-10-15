using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Trait")]
public class TraitData : ScriptableObject
{
    public string traitId;
    public string displayName;
    [TextArea] public string description;

    [Header("Eligibility")]
    [Tooltip("Для какого типажа возможна особенность. При пустом списке - доступны все")]
    public List<string> allowedArchetypes;
    [Tooltip("Веса для рандомизации")]
    public float weight = 1f;

    [Header("Effects")]
    [Tooltip("Эффект навыка")]
    public List<SkillEffectData> effects;
}
