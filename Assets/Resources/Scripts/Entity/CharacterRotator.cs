using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

public class CharacterRotator : MonoBehaviour, IRotator
{
    [SerializeField] private Transform model;
    [SerializeField] private float rotationSpeed = 1f;

    private PlayerMovementService _movement;
    private IInputService _input;
    private PlayerProfileService _profile;
    public Transform rotationModel => model;

    public void Start()
    {
        _movement = (PlayerMovementService) ServiceLocator.Instance.GetService<IPlayerMovementService>();
        _profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        rotationSpeed = _profile.CurrentProfile.rotationSpeed;
        _input = ServiceLocator.Instance.GetService<IInputService>();
        if (model == null) model = transform; // или hips
    }

    private void Update()
    {
        Vector3 targetDir = _input.GetMouseWorldDirection(Camera.main, transform);
        Debug.DrawRay(transform.position, targetDir);
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            Vector3 euler = targetRot.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.Euler(euler), rotationSpeed * Time.deltaTime);
        }
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

    public void SetTarget(Transform target) => model = target;
}
