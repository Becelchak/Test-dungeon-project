using UnityEngine;

/// <summary>
/// Процедурный поворот головы/шеи к точке прицеливания (IAimingService.AimPoint).
/// В отличие от MultiAimConstraint: лимитирует yaw/pitch, не даёт перекоса
/// (roll), ось «лица» калибруется автоматически по 6 направлениям кости.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HeadLookController : MonoBehaviour
{
    [Header("Limits")]
    [Tooltip("Максимальный поворот головы влево/вправо, градусы")]
    [SerializeField] private float yawLimit = 55f;
    [Tooltip("Максимальный наклон головы вверх/вниз, градусы")]
    [SerializeField] private float pitchLimit = 35f;

    [Header("Blend")]
    [Range(0f, 1f)]
    [Tooltip("Максимальный вес поворота головы (0 — выкл, 1 — полное следование)")]
    [SerializeField] private float maxWeight = 0.75f;
    [SerializeField] private float weightLerpSpeed = 6f;
    [Tooltip("Доля общего поворота, отдаваемая шее (остальное — голове)")]
    [Range(0f, 1f)]
    [SerializeField] private float neckShare = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = false;

    private Animator _animator;
    private IAimingService _aiming;
    private Transform _neck;
    private Transform _head;
    private float _weight;

    /// <summary>Локальная ось кости головы, смотрящая «лицом» (калибруется в Start).</summary>
    private Vector3 _faceAxisLocal = Vector3.forward;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _aiming = ServiceLocator.Instance.GetService<IAimingService>();

        _neck = _animator.GetBoneTransform(HumanBodyBones.Neck);
        _head = _animator.GetBoneTransform(HumanBodyBones.Head);
        if (_head == null) { enabled = false; return; }

        _faceAxisLocal = DetectFaceAxis();
    }

    /// <summary>
    /// Определяет, какая локальная ось кости головы соответствует направлению
    /// «лица»: перебирает ±X/±Y/±Z и выбирает ближайшую к forward персонажа.
    /// </summary>
    private Vector3 DetectFaceAxis()
    {
        Vector3 charFwd = transform.forward;
        Vector3 bestAxis = Vector3.forward;
        float bestScore = float.MinValue;

        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        foreach (var axis in axes)
        {
            foreach (float sign in new[] { 1f, -1f })
            {
                Vector3 local = axis * sign;
                Vector3 world = _head.TransformDirection(local);
                float score = Vector3.Dot(world, charFwd);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAxis = local;
                }
            }
        }
        return bestAxis;
    }

    private void LateUpdate()
    {
        if (_aiming == null || _head == null) return;

        _weight = Mathf.MoveTowards(_weight, maxWeight, weightLerpSpeed * Time.deltaTime);
        if (_weight < 0.01f) return;

        // Текущее направление «взгляда» головы
        Vector3 faceDir = _head.TransformDirection(_faceAxisLocal);
        Vector3 headPos = _head.position;
        Vector3 toTarget = _aiming.AimPoint - headPos;
        if (toTarget.sqrMagnitude < 0.0001f) return;
        Vector3 targetDir = toTarget.normalized;

        // ─── Yaw: горизонтальный угол от текущего взгляда к цели ───
        Vector3 faceFlat = Vector3.ProjectOnPlane(faceDir, Vector3.up).normalized;
        Vector3 targetFlat = Vector3.ProjectOnPlane(targetDir, Vector3.up).normalized;
        float yaw = 0f;
        if (faceFlat.sqrMagnitude > 0.001f && targetFlat.sqrMagnitude > 0.001f)
            yaw = Mathf.Clamp(Vector3.SignedAngle(faceFlat, targetFlat, Vector3.up), -yawLimit, yawLimit);

        // ─── Pitch: разница углов возвышения ───
        float facePitch = Mathf.Asin(Mathf.Clamp(faceDir.y, -1f, 1f)) * Mathf.Rad2Deg;
        float targetPitch = Mathf.Asin(Mathf.Clamp(targetDir.y, -1f, 1f)) * Mathf.Rad2Deg;
        float pitch = Mathf.Clamp(targetPitch - facePitch, -pitchLimit, pitchLimit);

        if (Mathf.Abs(yaw) < 0.05f && Mathf.Abs(pitch) < 0.05f) return;

        // Мировая дельта: yaw вокруг world up, pitch вокруг right персонажа.
        // pitch > 0 (цель выше) → вращаем «лицо» вверх → угол отрицательный вокруг right.
        Vector3 rightAxis = Vector3.Cross(Vector3.up, faceFlat);
        if (rightAxis.sqrMagnitude < 0.001f) rightAxis = transform.right;
        rightAxis.Normalize();

        Quaternion worldDelta = Quaternion.AngleAxis(yaw, Vector3.up)
                              * Quaternion.AngleAxis(-pitch, rightAxis);

        ApplyBoneDelta(_neck, worldDelta, neckShare * _weight);
        ApplyBoneDelta(_head, worldDelta, (1f - neckShare) * _weight);

        if (drawDebug)
        {
            Debug.DrawLine(headPos, _aiming.AimPoint, Color.yellow);
            Debug.DrawRay(headPos, faceDir * 0.5f, Color.cyan);
            Debug.DrawRay(headPos, targetDir * 0.5f, Color.green);
        }
    }

    /// <summary>Поворачивает кость на worldDelta (доля weight), без накопления ошибок.</summary>
    private void ApplyBoneDelta(Transform bone, Quaternion worldDelta, float weight)
    {
        if (bone == null || weight < 0.01f) return;

        // bone.rotation — мировая ориентация; дельта применяется в world space
        Quaternion worldTarget = Quaternion.Slerp(Quaternion.identity, worldDelta, weight) * bone.rotation;
        bone.rotation = worldTarget;
    }
}
