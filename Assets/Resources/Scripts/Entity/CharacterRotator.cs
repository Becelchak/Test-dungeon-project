using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotator : MonoBehaviour, IRotator
{
    [SerializeField] private Transform model;
    private Transform targetRotation;

    public void Start()
    {
        var servise = (PlayerMovementService)ServiceLocator.Instance.GetService<IPlayerMovementService>();
        model = servise.Hips;
    }

    public void RotateTowards(Vector3 direction, float rotationSpeed)
    {
        var directionRotation = Quaternion.LookRotation(direction);

        model.transform.rotation = Quaternion.Slerp(
            model.transform.rotation,
            directionRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Transform target)
    {
        targetRotation = target;
    }
}
