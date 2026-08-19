using EventBusSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

public class CharacterRotator : MonoBehaviour, IRotator, IPlayerDiedEventSubscriber
{
    [SerializeField] private Transform model;
    [SerializeField] private float rotationSpeed = 1f;

    private IInputService _input;
    private PlayerProfileService _profile;
    private bool _isCharacterDeath = false;
    public Transform rotationModel => model;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    public void Start()
    {
        _profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        rotationSpeed = _profile.CurrentProfile.rotationSpeed;
        _input = ServiceLocator.Instance.GetService<IInputService>();
        if (model == null) model = transform; // или hips
    }

    private void Update()
    {
        if (_isCharacterDeath) return;
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

    public void OnPlayerDied(PlayerDiedEvent evt)
    {
        _isCharacterDeath = true;
    }

    public void OnRevivePlayer()
    {
        _isCharacterDeath = false;
    }
}
