using UnityEngine;

public interface IAimingService
{
    Vector3 AimPoint { get; }
    Vector3 AimDirection { get; }
    bool HasTarget { get; }
}