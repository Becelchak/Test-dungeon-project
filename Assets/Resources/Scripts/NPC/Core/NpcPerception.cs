using UnityEngine;

/// <summary>
/// Система восприятия NPC. Ищет игрока в радиусе/угле обзора и проверяет дистанцию для атаки.
/// </summary>
public class NpcPerception : MonoBehaviour
{
    [Tooltip("Слой, на котором находится игрок")]
    [SerializeField] private LayerMask _playerLayer;

    [Tooltip("Интервал обновления поиска цели (сек)")]
    [SerializeField] private float _updateInterval = 0.2f;

    [Tooltip("Выводить ли отладочные сообщения о поиске цели")]
    [SerializeField] private bool _debugLog;

    [Tooltip("ВРЕМЕННО: игнорировать проверку прямой видимости. Полезно для диагностики.")]
    [SerializeField] private bool _ignoreLineOfSight;

    private Transform _currentTarget;
    private float _updateTimer;
    private NpcController _controller;

    private void Start()
    {
        if (_playerLayer == 0)
            Debug.LogWarning($"[NpcPerception] На {gameObject.name} не назначен _playerLayer. NPC не сможет обнаружить игрока.");

        var data = _controller?.Data;
        if (data != null)
        {
            if (data.detectionRadius <= 0)
                Debug.LogWarning($"[NpcPerception] На {gameObject.name} detectionRadius <= 0. NPC не сможет обнаружить игрока.");
            if (data.detectionAngle <= 0)
                Debug.LogWarning($"[NpcPerception] На {gameObject.name} detectionAngle <= 0. NPC не сможет обнаружить игрока.");
        }
    }

    public Transform CurrentTarget => _currentTarget;
    public bool HasTarget => _currentTarget != null;
    public bool IsTargetInAttackRange { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<NpcController>();
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
    }

    public void ClearTarget()
    {
        if (_currentTarget != null && _debugLog)
            Debug.Log($"[NpcPerception] {gameObject.name} потерял цель.");

        _currentTarget = null;
        IsTargetInAttackRange = false;
    }

    private void Update()
    {
        _updateTimer += Time.deltaTime;
        if (_updateTimer < _updateInterval) return;
        _updateTimer = 0f;

        if (_currentTarget == null)
            SearchForTarget();
        else
            EvaluateCurrentTarget();
    }

    private void SearchForTarget()
    {
        var data = _controller?.Data;
        if (data == null) return;

        if (_debugLog)
            Debug.Log($"[NpcPerception] {gameObject.name} ищет цель. Радиус={data.detectionRadius}, Угол={data.detectionAngle}, Слой={_playerLayer.value}, Позиция={transform.position}");

        Collider[] results = new Collider[32];
        int count = Physics.OverlapSphereNonAlloc(transform.position, data.detectionRadius, results, _playerLayer);

        if (_debugLog)
            Debug.Log($"[NpcPerception] {gameObject.name} нашёл {count} коллайдер(ов) в радиусе.");

        for (int i = 0; i < count; i++)
        {
            var candidate = results[i].transform;
            if (_debugLog)
                Debug.Log($"[NpcPerception] {gameObject.name} кандидат [{i}]: {candidate.name}, слой={LayerMask.LayerToName(candidate.gameObject.layer)}");

            if (!IsInFieldOfView(candidate, data.detectionAngle))
            {
                if (_debugLog)
                    Debug.Log($"[NpcPerception] {gameObject.name}: {candidate.name} отброшен — вне поля зрения.");
                continue;
            }

            if (!_ignoreLineOfSight && !HasLineOfSight(candidate))
            {
                if (_debugLog)
                    Debug.Log($"[NpcPerception] {gameObject.name}: {candidate.name} отброшен — нет прямой видимости.");
                continue;
            }

            _currentTarget = candidate;
            if (_debugLog)
                Debug.Log($"[NpcPerception] {gameObject.name} обнаружил цель: {candidate.name}");
            return;
        }
    }

    private void EvaluateCurrentTarget()
    {
        var data = _controller?.Data;
        if (data == null) return;

        float sqrDistance = (_currentTarget.position - transform.position).sqrMagnitude;
        IsTargetInAttackRange = sqrDistance <= data.attackRange * data.attackRange;

        if (sqrDistance > data.detectionRadius * data.detectionRadius)
            ClearTarget();
    }

    private bool IsInFieldOfView(Transform target, float angle)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, direction);
        float halfAngle = angle * 0.5f;
        return Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg <= halfAngle;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        var hits = Physics.RaycastAll(origin, direction.normalized, distance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // Пропускаем собственные коллайдеры
            if (hit.transform.root == transform.root) continue;

            // Если первым препятствием является сама цель или её часть — LOS есть
            if (hit.transform == target || hit.transform.IsChildOf(target)) return true;

            // Иначе что-то другое заслоняет цель
            if (_debugLog)
                Debug.Log($"[NpcPerception] {gameObject.name}: LOS заблокирован {hit.transform.name}");
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        var data = _controller?.Data;
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}
