using UnityEngine;

public interface IAimingService
{
    Vector3 AimPoint { get; }
    Vector3 AimDirection { get; }
    bool HasTarget { get; }
    /// <summary>Точка прицеливания, спроецированная на уровень глаз.</summary>
    Vector3 EyeLevelPoint { get; }
    /// <summary>true — прицел не на цели (ignored-слои/пустота), AimPoint на уровне глаз.</summary>
    bool IsEyeLevelAim { get; }
}