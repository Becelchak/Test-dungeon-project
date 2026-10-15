using System;
using EventBusSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionTargetingService : BaseService, IInteractionTargetingService
{
    [Header("Settings")]
    [SerializeField] private LayerMask interactableMask;
    [Tooltip("Максимальная дистанция от игрока до цели взаимодействия")]
    [SerializeField] private float maxDistance = 3f;

    private IInteractable _current;
    private Camera _camera;
    private IInputService _input;
    private Transform _playerTransform;

    public event Action<IInteractable> OnTargetChanged;
    public event Action OnTargetLost;

    public IInteractable CurrentTarget => _current;
    public Vector3 TargetWorldPosition { get; private set; }

    protected override Type GetServiceType() => typeof(IInteractionTargetingService);

    private void Start()
    {
        _camera = Camera.main;
        _input = ServiceLocator.Instance.GetService<IInputService>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (_camera == null || _playerTransform == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out var hit, 100f, interactableMask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            float dist = Vector3.Distance(_playerTransform.position, hit.point);

            if (interactable != null && dist <= maxDistance && interactable.CanInteract(_playerTransform.gameObject))
            {
                SetTarget(interactable, hit.point);
                return;
            }
        }
        ClearTarget();
    }

    private void SetTarget(IInteractable t, Vector3 pos)
    {
        if (_current == t) return;
        _current = t;
        TargetWorldPosition = pos;
        EventBus.RaiseEvent<IInteractionTargetChangedSubscriber>(s => s.OnTargetChanged(t));
    }

    private void ClearTarget()
    {
        if (_current == null) return;
        _current = null;
        EventBus.RaiseEvent<IInteractionTargetLostSubscriber>(s => s.OnTargetLost());
    }
}
