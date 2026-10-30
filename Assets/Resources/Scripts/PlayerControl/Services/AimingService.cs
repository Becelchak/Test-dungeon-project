using UnityEngine;

public class AimingService : BaseService, IAimingService
{
    [SerializeField] private LayerMask aimLayers;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool debugDraw = true;
    [SerializeField] private bool debugLog = false;

    private Camera _camera;
    private Transform _headTransform;

    public Vector3 AimPoint { get; private set; }
    public Vector3 AimDirection { get; private set; }
    public bool HasTarget { get; private set; }

    protected override System.Type GetServiceType() => typeof(IAimingService);

    private void Start()
    {
        _camera = Camera.main;
        if (playerTransform == null)
        {
            var mv = ServiceLocator.Instance.GetService<IPlayerMovementService>();
            playerTransform = mv is MonoBehaviour m ? m.transform : transform;
        }
        
        // Поиск головы персонажа
        var animator = playerTransform.GetComponent<Animator>();
        if (animator != null)
        {
            _headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        
        if (aimLayers == 0)
        {
            if(debugLog)
                Debug.LogWarning("[AimingService] aimLayers не настроен! Используем Default.");
            aimLayers = LayerMask.GetMask("Default");
        }
    }

    private void Update()
    {
        if (_camera == null) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        // RaycastAll для приоритета NpcHitbox
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, aimLayers, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        HasTarget = false;
        AimPoint = ray.origin + ray.direction * maxDistance;

        foreach (var hit in hits)
        {
            if (hit.transform.root == playerTransform.root) continue;

            if (hit.collider.GetComponent<NpcHitbox>() != null)
            {
                AimPoint = hit.point;
                HasTarget = true;
                break;
            }
        }

        // Если NpcHitbox не найден - берется ближайшее попадание
        if (!HasTarget && hits.Length > 0)
        {
            AimPoint = hits[0].point;
            HasTarget = true;
        }

        // КОРРЕКЦИЯ от головы персонажа
        if (HasTarget && _headTransform != null)
        {
            Vector3 headPos = _headTransform.position;
            Vector3 toAim = AimPoint - headPos;
            float headDist = toAim.magnitude;
            
            float maxHeadDist = 15f;
            if (headDist > maxHeadDist)
            {
                AimPoint = headPos + toAim.normalized * maxHeadDist;
            }
            
            // Отрисовка линии от головы персонада
            if (debugDraw)
            {
                Debug.DrawLine(headPos, AimPoint, Color.red);
            }
        }

        // Отрисовка для отладки
        if (debugDraw)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan);
            Debug.DrawRay(AimPoint, Vector3.up * 0.5f, Color.magenta);
            Debug.DrawRay(AimPoint + Vector3.left * 0.2f, Vector3.right * 0.4f, Color.magenta);
            Debug.DrawRay(AimPoint + Vector3.back * 0.2f, Vector3.forward * 0.4f, Color.magenta);
            
            if (hits.Length > 0)
            {
                string hitName = hits[0].collider.name;
                string hitLayer = LayerMask.LayerToName(hits[0].collider.gameObject.layer);
                if (debugLog)
                    Debug.Log($"[AimingService] Hits: {hits.Length}, First: {hitName} (Layer: {hitLayer}), AimPoint: {AimPoint}");
            }
            else
            {
                if (debugLog)
                    Debug.Log($"[AimingService] No hits! AimPoint: {AimPoint}");
            }
        }

        // Направление для поворота тела — плоское, от игрока
        Vector3 flat = AimPoint - playerTransform.position;
        flat.y = 0f;
        AimDirection = flat.sqrMagnitude > 0.0001f ? flat.normalized : playerTransform.forward;
    }
}
