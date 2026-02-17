using UnityEngine;

public interface IRotator
{
    void RotateTowards(Vector3 direction, float rotationSpeed);
    void SetTarget(Transform target);
}