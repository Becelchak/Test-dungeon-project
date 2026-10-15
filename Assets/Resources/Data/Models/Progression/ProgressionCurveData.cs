using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Curve")]
public class ProgressionCurveData : ScriptableObject
{
    public int maxLevel = 40;
    public int maxXP = 40000;
    public int baseXP = 100;
    [Tooltip("Кол-во опыта, необходимое для каждого отдельного уровня")]
    public AnimationCurve xpRequiredPerLevel;
    public int skillPointsPerLevel = 1;
    public int maxSanityMin = 20;
    public int maxSanityMax = 100;
    public float sanityCurve = 0.5f;

    public int GetXPForLevel(int level)
    {
        if (level > maxLevel) return -1;
        float t = (float)(level - 1) / (maxLevel - 1);
        return Mathf.RoundToInt(baseXP + xpRequiredPerLevel.Evaluate(t) * (maxXP - baseXP));
    }
    public int GetMaxSanity(int level) => Mathf.FloorToInt(maxSanityMin + (maxSanityMax - maxSanityMin) * (Mathf.Pow((1 - (level - 1) / (maxLevel - 1)), sanityCurve)));
}