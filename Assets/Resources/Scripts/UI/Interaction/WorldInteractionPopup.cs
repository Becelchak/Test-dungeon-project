using EventBusSystem;
using UnityEngine;

public class WorldInteractionPopup : MonoBehaviour,
    IInteractionTargetChangedSubscriber,
    IInteractionTargetLostSubscriber
{
    [Header("UI")]
    [SerializeField] private RectTransform root;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;

    private Camera _camera;
    private Transform _targetTransform;

    private void Awake()
    {
        _camera = Camera.main;
        if (root != null) root.gameObject.SetActive(false);
    }

    private void Start()
    {
        EventBus.Subscribe(this);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(this);
    }

    public void OnTargetChanged(IInteractable t)
    {
        if (t == null) return;

        nameText.text = t.GetDisplayName();
        promptText.text = t.GetInteractionPrompt();
        _targetTransform = t.Transform;

        if (root != null) root.gameObject.SetActive(true);
    }

    public void OnTargetLost()
    {
        if (root != null) root.gameObject.SetActive(false);
        _targetTransform = null;
    }

    private void LateUpdate()
    {
        if (_targetTransform == null || _camera == null) return;

        Vector3 screen = _camera.WorldToScreenPoint(_targetTransform.position + Vector3.up * 2f);
        root.position = screen;
    }
}
