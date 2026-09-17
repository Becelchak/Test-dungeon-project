using UnityEngine;

[CreateAssetMenu(fileName = "HudStatBarConfig", menuName = "Game/Stat Bar Config")]
public class HudStatBarConfig : ScriptableObject
{
    [Header("Auto-Hide")]
    [Tooltip("Скрывать ли полосу, если она давно не менялась")]
    public bool autoHide = true;

    [Tooltip("Сколько секунд полоса остаётся видимой после последнего изменения")]
    [Min(0f)] public float idleTimeout = 3f;

    [Tooltip("Длительность плавного появления/исчезновения")]
    [Min(0.01f)] public float fadeDuration = 0.4f;

    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Delayed Bar")]
    [Tooltip("Скорость, с которой отстающая полоса догоняет основную (единиц/сек)")]
    [Min(0.01f)] public float delayedBarSpeed = 0.5f;

    [Header("Critical Threshold (optional)")]
    [Tooltip("Если значение ниже этого % от максимума — полоса не скрывается")]
    [Range(0f, 1f)] public float criticalThreshold = 0f;
}