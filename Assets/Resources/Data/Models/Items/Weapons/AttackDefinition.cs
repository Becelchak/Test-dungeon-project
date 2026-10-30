using UnityEngine;

public enum MeleeAttackType
{
    Slash,
    Thrust
}

[CreateAssetMenu(
    fileName = "AttackDefinition",
    menuName = "Game/Combat/AttackDefinition",
    order = 0)]
public class AttackDefinition : ScriptableObject
{
    [Header("Animation")]
    public AnimationClip clip;

    [Header("Type")]
    public MeleeAttackType type = MeleeAttackType.Slash;

    [Header("Slash — Body Aim")]
    [Range(0f, 1f)]
    [Tooltip("Насколько сильно корпус наклоняется к цели по высоте (pitch). " +
             "0.9 — заметно, 1 — полностью. Компенсирует промахи по голове.")]
    public float slashBodyAimWeight = 0.9f;

    [Header("Thrust — Impact Window (нормализованное время клипа 0..1)")]
    [Range(0f, 1f)]
    [Tooltip("Начало активной фазы удара (где IK включается на полную).")]
    public float thrustImpactStart = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("Конец активной фазы удара (где IK начинает гаснуть).")]
    public float thrustImpactEnd = 0.6f;

    [Range(0.05f, 0.3f)]
    [Tooltip("Длительность плавного включения/выключения IK вокруг окна.")]
    public float thrustWeightRamp = 0.1f;

    private void OnValidate()
    {
        thrustImpactEnd = Mathf.Max(thrustImpactStart + 0.05f, thrustImpactEnd);
    }
}