using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// Типы урона, используемые в боевой системе.
/// </summary>
public enum DamageType
{
    [Tooltip("Режущий")]
    Slashing,
    [Tooltip("Колющий")]
    Piercing,
    [Tooltip("Дробящий")]
    Blunt,
    Fire,
    Frost,
    Poison,
    Lightning,
    True
}
/// <summary>
/// Семейство урона (обобщение)
/// </summary>
public enum DamageFamily
{
    Physical,   // Slashing + Piercing + Blunt
    Elemental,  // Fire + Frost + Lightning
    Occult,     // Poison
}
