using UnityEngine.Animations.Rigging;
using UnityEngine;
using System.Linq;

public class WeaponIKController : MonoBehaviour
{
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform weaponHolder_R;
    [SerializeField] private Transform weaponHolder_L;
    [Tooltip("Parent for two-handed weapon in IDLE. Usually Hips/Spine/Root. " +
             "If not assigned, two-handed weapon stays attached to WeaponHolder_R.")]
    [SerializeField] private Transform weaponIdleParent;
    [SerializeField] private TwoBoneIKConstraint rightGrip;
    [SerializeField] private TwoBoneIKConstraint leftGrip;
    [SerializeField] private Transform rightHandTargetEmpty;
    [SerializeField] private Transform leftHandTargetEmpty;

    [Header("IK Smoothing")]
    [Tooltip("Speed of IK weight blending")]
    [SerializeField] private float weightLerpSpeed = 7f;
    [Tooltip("Time to smoothly return weapon to IDLE pose after attack")]
    [SerializeField] private float idleReturnDuration = 0.15f;

    [Header("Runtime")]
    [Tooltip("If true, right hand also reaches for RightGrip. " +
             "Only relevant if weaponIdleParent is not assigned.")]
    [SerializeField] private bool useRightHandIK = false;

    /// <summary>
    /// Текущая боевая поза оружия. Влияет на родителя оружия,
    /// IK-веса и смещения для двуручного хвата.
    /// </summary>
    public enum WeaponPose { Idle, Block, Parry, Attack }

    private IEquipmentService _equipment;
    private GameObject _currentWeaponObject;
    private WeaponData _currentWeaponData;
    private GameObject _currentShieldObject;
    private WeaponData _currentShieldData;

    /// <summary>Текущий runtime-объект оружия в руках персонажа.</summary>
    public GameObject CurrentWeaponObject => _currentWeaponObject;

    /// <summary>Источник урона текущего runtime-оружия (ищется в дочерних объектах).</summary>
    public WeaponDamageSource CurrentWeaponDamageSource =>
        _currentWeaponObject != null
            ? _currentWeaponObject.GetComponentInChildren<WeaponDamageSource>(true)
            : null;

    /// <summary>Animator персонажа, используемый WeaponIKController.</summary>
    public Animator PlayerAnimator => animator;

    private float _currentRightWeight;
    private float _currentLeftWeight;
    private float _targetRightWeight;
    private float _targetLeftWeight;

    private WeaponPose _currentPose = WeaponPose.Idle;
    private Coroutine _transitionCoroutine;

    private Camera _mainCamera;
    private float _currentAimPitch;
    private float _targetAimPitch;
    private Quaternion _aimRotation = Quaternion.identity;
    private Vector3 _currentAimPoint;

    [Header("Debug Aiming")]
    [Tooltip("Показывать ли временную визуализацию точки прицеливания.")]
    [SerializeField] private bool _showAimDebug = true;
    [Tooltip("Размер маркера точки прицеливания.")]
    [SerializeField] private float _aimMarkerSize = 0.08f;
    [Tooltip("Цвет линии от оружия к точке прицеливания.")]
    [SerializeField] private Color _aimLineColor = Color.yellow;
    [Tooltip("Цвет маркера точки прицеливания.")]
    [SerializeField] private Color _aimMarkerColor = Color.red;

    private LineRenderer _aimLine;
    private Transform _aimMarkerTransform;

    private void Start()
    {
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        if (_equipment == null)
        {
            Debug.LogError("[WeaponIKController] EquipmentService not found!");
            enabled = false;
            return;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _mainCamera = Camera.main;
        if (_mainCamera == null)
            Debug.LogWarning("[WeaponIKController] Main camera not found. Aiming will not work.");

        EnsureRigConstraints();
        CreateDebugAimObjects();

        _currentRightWeight = rightGrip != null ? rightGrip.weight : 0f;
        _currentLeftWeight = leftGrip != null ? leftGrip.weight : 0f;

        _equipment.OnWeaponChanged += OnWeaponChanged;
        _equipment.OnShieldChanged += OnShieldChanged;
        OnWeaponChanged(_equipment.CurrentWeapon);
    }

    private void Update()
    {
        _currentRightWeight = Mathf.MoveTowards(
            _currentRightWeight, _targetRightWeight, Time.deltaTime * weightLerpSpeed);
        _currentLeftWeight = Mathf.MoveTowards(
            _currentLeftWeight, _targetLeftWeight, Time.deltaTime * weightLerpSpeed);

        if (rightGrip != null)
            rightGrip.weight = _currentRightWeight;
        if (leftGrip != null)
            leftGrip.weight = _currentLeftWeight;
    }

    private void LateUpdate()
    {
        if (_currentWeaponObject == null || _currentWeaponData == null)
            return;

        if (_currentPose == WeaponPose.Attack)
            UpdateAimPitch();
        else
            _targetAimPitch = 0f;

        float speed = _currentWeaponData.aimSpeed > 0f ? _currentWeaponData.aimSpeed : 360f;
        _currentAimPitch = Mathf.MoveTowards(_currentAimPitch, _targetAimPitch, Time.deltaTime * speed);
        _aimRotation = Quaternion.AngleAxis(_currentAimPitch, GetAimAxis());

        ApplyAimRotation();
        UpdateDebugAimVisuals();
    }

    private Vector3 GetAimAxis()
    {
        if (_currentWeaponData == null)
            return Vector3.right;

        Vector3 axis = _currentWeaponData.aimRotationAxis;
        return axis.sqrMagnitude < 0.0001f ? Vector3.right : axis.normalized;
    }

    /// <summary>
    /// Вычисляет целевой вертикальный угол оружия на основе положения курсора.
    /// </summary>
    private void UpdateAimPitch()
    {
        if (_mainCamera == null)
        {
            _targetAimPitch = 0f;
            return;
        }

        if (_currentWeaponData.aimLayers == 0)
        {
            _targetAimPitch = 0f;
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        float maxDistance = _currentWeaponData.Stats != null && _currentWeaponData.Stats.range > 0f
            ? _currentWeaponData.Stats.range
            : 50f;

        Vector3 aimPoint = GetAimPointFromCursor(ray, maxDistance);
        _currentAimPoint = aimPoint;

        Transform weaponTransform = _currentWeaponObject.transform;
        Vector3 pivotWorld = weaponTransform.TransformPoint(_currentWeaponData.aimPivotOffset);
        Vector3 toTarget = aimPoint - pivotWorld;

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            _targetAimPitch = 0f;
            return;
        }

        toTarget.Normalize();

        // Вычисляем pitch относительно вертикальной плоскости взгляда персонажа
        Transform characterRoot = transform;
        Vector3 forward = characterRoot.forward;
        Vector3 up = characterRoot.up;

        float y = Vector3.Dot(toTarget, up);
        float z = Vector3.Dot(toTarget, forward);
        float pitch = Mathf.Atan2(y, z) * Mathf.Rad2Deg;

        pitch = Mathf.Clamp(pitch,
            -_currentWeaponData.maxAimAngleDown,
            _currentWeaponData.maxAimAngleUp);

        _targetAimPitch = pitch;
    }

    /// <summary>
    /// Бросает луч из камеры через курсор и возвращает мировую точку прицеливания.
    /// Приоритет отдаётся коллайдерам с NpcHitbox, чтобы можно было целиться в конкретные части тела.
    /// </summary>
    private Vector3 GetAimPointFromCursor(Ray ray, float maxDistance)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            ray, maxDistance, _currentWeaponData.aimLayers, QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            if (hit.collider.GetComponent<NpcHitbox>() != null)
                return hit.point;
        }

        if (hits.Length > 0)
            return hits[0].point;

        return ray.origin + ray.direction * maxDistance;
    }

    /// <summary>
    /// Применяет дополнительный поворот оружия поверх базовой позы Idle/Attack/Block.
    /// </summary>
    private void ApplyAimRotation()
    {
        if (_currentWeaponObject == null || _currentWeaponData == null)
            return;

        Transform weaponTransform = _currentWeaponObject.transform;
        bool useCombatPose = ShouldUseCombatPose();

        Quaternion baseRotation = Quaternion.Euler(useCombatPose
            ? _currentWeaponData.weaponHolderAttackRotationOffsetEuler
            : _currentWeaponData.weaponHolderRotationOffsetEuler);

        weaponTransform.localRotation = baseRotation * _aimRotation;
    }

    private bool ShouldUseCombatPose()
    {
        if (_currentWeaponData == null)
            return false;

        bool isBothHands = _currentWeaponData.handling == WeaponHandling.BothHands;
        return _currentPose == WeaponPose.Attack
            || (isBothHands && (_currentPose == WeaponPose.Block || _currentPose == WeaponPose.Parry));
    }

    /// <summary>
    /// Создаёт временные объекты для отладки прицеливания: линию и маркер.
    /// </summary>
    private void CreateDebugAimObjects()
    {
        var lineGo = new GameObject("AimDebugLine");
        lineGo.transform.SetParent(transform, false);
        _aimLine = lineGo.AddComponent<LineRenderer>();
        _aimLine.useWorldSpace = true;
        _aimLine.positionCount = 2;
        _aimLine.startWidth = 0.015f;
        _aimLine.endWidth = 0.015f;
        _aimLine.material = new Material(Shader.Find("Sprites/Default"));
        _aimLine.startColor = _aimLineColor;
        _aimLine.endColor = _aimLineColor;
        _aimLine.enabled = false;

        var markerGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        markerGo.name = "AimDebugMarker";
        markerGo.transform.SetParent(transform, false);
        markerGo.transform.localScale = Vector3.one * _aimMarkerSize;

        var collider = markerGo.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        var renderer = markerGo.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = _aimMarkerColor;

        _aimMarkerTransform = markerGo.transform;
        _aimMarkerTransform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Обновляет временную визуализацию точки прицеливания.
    /// </summary>
    private void UpdateDebugAimVisuals()
    {
        bool show = _showAimDebug
            && _currentPose == WeaponPose.Attack
            && _currentWeaponObject != null
            && _currentWeaponData != null;

        if (_aimLine != null)
            _aimLine.enabled = show;

        if (_aimMarkerTransform != null)
            _aimMarkerTransform.gameObject.SetActive(show);

        if (!show)
            return;

        Transform weaponTransform = _currentWeaponObject.transform;
        Vector3 pivotWorld = weaponTransform.TransformPoint(_currentWeaponData.aimPivotOffset);

        if (_aimLine != null)
        {
            _aimLine.SetPosition(0, pivotWorld);
            _aimLine.SetPosition(1, _currentAimPoint);
        }

        if (_aimMarkerTransform != null)
            _aimMarkerTransform.position = _currentAimPoint;
    }

    private void OnDestroy()
    {
        if (_equipment != null)
        {
            _equipment.OnWeaponChanged -= OnWeaponChanged;
            _equipment.OnShieldChanged -= OnShieldChanged;
        }
    }

    public void SetIKWeights(float rightWeight, float leftWeight, bool instantly = false)
    {
        _targetRightWeight = rightWeight;
        _targetLeftWeight = leftWeight;

        if (instantly)
        {
            _currentRightWeight = rightWeight;
            _currentLeftWeight = leftWeight;

            if (rightGrip != null)
                rightGrip.weight = rightWeight;
            if (leftGrip != null)
                leftGrip.weight = leftWeight;
        }
    }

    public (float right, float left) GetTargetWeightsFor(WeaponHandling handling, bool useCombatPose)
    {
        return handling switch
        {
            WeaponHandling.RightHand => (useRightHandIK && !useCombatPose ? 1f : 0f, 0f),
            WeaponHandling.LeftHand => (0f, useRightHandIK && !useCombatPose ? 1f : 0f),
            WeaponHandling.BothHands => useCombatPose ? (0f, 0.285f) : (1f, 1f),
            WeaponHandling.OffHand => (0f, 0f),
            _ => (0f, 0f)
        };
    }

    /// <summary>
    /// Возвращает IK-веса для текущей позы оружия.
    /// </summary>
    public (float right, float left) GetTargetWeightsFor(WeaponHandling handling)
    {
        bool isBothHands = handling == WeaponHandling.BothHands;
        bool useCombatPose = _currentPose == WeaponPose.Attack
            || (isBothHands && (_currentPose == WeaponPose.Block || _currentPose == WeaponPose.Parry));
        return GetTargetWeightsFor(handling, useCombatPose);
    }

    /// <summary>
    /// Переключает текущую позу оружия (Idle, Block, Parry, Attack).
    /// Для двуручного оружия Block/Parry используют боевую позу, как и Attack.
    /// </summary>
    public void SetPose(WeaponPose pose, bool instantly = false)
    {
        _currentPose = pose;
        ApplyCurrentPose(instantly);
    }

    /// <summary>
    /// Применяет текущую позу оружия на основе _currentPose.
    /// Гарантирует корректные переходы между Idle, Block, Parry и Attack для двуручного оружия.
    /// </summary>
    private void ApplyCurrentPose(bool instantly = false)
    {
        if (_currentWeaponObject == null || _currentWeaponData == null)
            return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        bool isBothHands = _currentWeaponData.handling == WeaponHandling.BothHands;

        // Для двуручного оружия Block/Parry/Attack используют одну и ту же "боевую" позу.
        // Для одноручного — только Attack смещает оружие.
        bool useCombatPose = _currentPose == WeaponPose.Attack
            || (isBothHands && (_currentPose == WeaponPose.Block || _currentPose == WeaponPose.Parry));

        if (isBothHands && weaponIdleParent != null)
        {
            Transform desiredParent = useCombatPose ? weaponHolder_R : weaponIdleParent;
            if (desiredParent != null && _currentWeaponObject.transform.parent != desiredParent)
                _currentWeaponObject.transform.SetParent(desiredParent, worldPositionStays: true);
        }

        ApplyWeaponTransform(_currentWeaponObject.transform, useCombatPose);

        if (rigBuilder != null)
            rigBuilder.Build();

        var (rightWeight, leftWeight) = GetTargetWeightsFor(_currentWeaponData.handling, useCombatPose);
        SetIKWeights(rightWeight, leftWeight, instantly);
    }

    private void ApplyWeaponTransform(Transform weaponTransform, bool useCombatPose)
    {
        if (weaponTransform == null || _currentWeaponData == null) return;

        Vector3 targetPos = useCombatPose
            ? _currentWeaponData.weaponHolderAttackOffset
            : _currentWeaponData.weaponHolderOffset;
        Quaternion targetRot = Quaternion.Euler(useCombatPose
            ? _currentWeaponData.weaponHolderAttackRotationOffsetEuler
            : _currentWeaponData.weaponHolderRotationOffsetEuler);

        weaponTransform.localPosition = targetPos;
        weaponTransform.localRotation = targetRot;
    }

    private System.Collections.IEnumerator TransitionToIdlePose()
    {
        if (_currentWeaponObject == null || _currentWeaponData == null)
            yield break;

        var t = _currentWeaponObject.transform;
        Vector3 startLocalPos = t.localPosition;
        Quaternion startLocalRot = t.localRotation;

        Vector3 targetLocalPos = _currentWeaponData.weaponHolderOffset;
        Quaternion targetLocalRot = Quaternion.Euler(_currentWeaponData.weaponHolderRotationOffsetEuler);

        float elapsed = 0f;
        while (elapsed < idleReturnDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / idleReturnDuration);
            float tValue = normalized * normalized * (3f - 2f * normalized);

            t.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, tValue);
            t.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, tValue);

            yield return null;
        }

        t.localPosition = targetLocalPos;
        t.localRotation = targetLocalRot;
        _transitionCoroutine = null;

        if (rigBuilder != null)
            rigBuilder.Build();
    }

    private void OnWeaponChanged(WeaponData weapon)
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        if (_currentWeaponObject != null)
            Destroy(_currentWeaponObject);

        if (_currentShieldObject != null)
            Destroy(_currentShieldObject);

        ClearHolders();
        _currentWeaponData = weapon;
        _currentShieldData = null;
        _currentPose = WeaponPose.Idle;
        _currentAimPitch = 0f;
        _targetAimPitch = 0f;
        _aimRotation = Quaternion.identity;

        if (weapon == null || weapon.weaponPrefab == null)
        {
            SetIKWeights(0f, 0f, true);
            RefreshShield();
            return;
        }

        Transform targetHolder = GetInitialParent(weapon.handling);
        if (targetHolder == null)
        {
            Debug.LogWarning($"[WeaponIKController] WeaponHolder not assigned for {weapon.handling}");
            SetIKWeights(0f, 0f, true);
            return;
        }

        _currentWeaponObject = Instantiate(weapon.weaponPrefab, targetHolder);
        _currentWeaponObject.transform.localPosition = weapon.weaponHolderOffset;
        _currentWeaponObject.transform.localRotation = Quaternion.Euler(weapon.weaponHolderRotationOffsetEuler);

        Transform rightGripTransform = _currentWeaponObject.transform.Find("RightGrip");
        Transform leftGripTransform = _currentWeaponObject.transform.Find("LeftGrip");

        if (rightGripTransform != null)
        {
            AttachIKTarget(rightHandTargetEmpty, rightGripTransform,
                weapon.rightHandPositionOffset,
                Quaternion.Euler(weapon.rightHandRotationOffsetEuler));
        }

        if (leftGripTransform != null)
        {
            AttachIKTarget(leftHandTargetEmpty, leftGripTransform,
                weapon.leftHandPositionOffset,
                Quaternion.Euler(weapon.leftHandRotationOffsetEuler));
        }

        var (rightWeight, leftWeight) = GetTargetWeightsFor(weapon.handling);

        if (weapon.handling == WeaponHandling.BothHands && leftGripTransform == null)
        {
            Debug.LogWarning($"[WeaponIKController] Two-handed weapon '{weapon.name}' has no LeftGrip. " +
                             "Left hand will not grip the weapon.");
            leftWeight = 0f;
        }

        SetIKWeights(rightWeight, leftWeight, true);

        RefreshShield();

        if (rigBuilder != null)
            rigBuilder.Build();
    }

    private void OnShieldChanged(WeaponData shield)
    {
        RefreshShield();
    }

    private void RefreshShield()
    {
        if (_currentShieldObject != null)
        {
            Destroy(_currentShieldObject);
            _currentShieldObject = null;
            _currentShieldData = null;
        }

        var shield = _equipment?.CurrentShield;
        if (shield == null || shield.weaponPrefab == null)
            return;

        if (_currentWeaponData == null)
            return;

        // Щит не отображается вместе с двуручным оружием
        if (_currentWeaponData.handling == WeaponHandling.BothHands)
            return;

        // Щит встаёт в руку, противоположную основному оружию
        Transform shieldHolder = _currentWeaponData.handling == WeaponHandling.RightHand
            ? weaponHolder_L
            : weaponHolder_R;

        if (shieldHolder == null)
        {
            Debug.LogWarning("[WeaponIKController] WeaponHolder для щита не назначен.");
            return;
        }

        _currentShieldData = shield;
        _currentShieldObject = Instantiate(shield.weaponPrefab, shieldHolder);
        _currentShieldObject.transform.localPosition = shield.weaponHolderOffset;
        _currentShieldObject.transform.localRotation = Quaternion.Euler(shield.weaponHolderRotationOffsetEuler);
    }

    private void AttachIKTarget(Transform target, Transform grip, Vector3 positionOffset, Quaternion rotationOffset)
    {
        if (target == null || grip == null)
            return;

        target.SetParent(grip);
        target.localPosition = positionOffset;
        target.localRotation = rotationOffset;
    }

    private Transform GetInitialParent(WeaponHandling handling)
    {
        return handling switch
        {
            WeaponHandling.RightHand => weaponHolder_R,
            WeaponHandling.LeftHand => weaponHolder_L,
            WeaponHandling.BothHands => weaponIdleParent != null ? weaponIdleParent : weaponHolder_R,
            _ => weaponHolder_R
        };
    }

    private void ClearHolders()
    {
        if (weaponHolder_R != null)
        {
            for (int i = weaponHolder_R.childCount - 1; i >= 0; i--)
                Destroy(weaponHolder_R.GetChild(i).gameObject);
        }

        if (weaponHolder_L != null)
        {
            for (int i = weaponHolder_L.childCount - 1; i >= 0; i--)
                Destroy(weaponHolder_L.GetChild(i).gameObject);
        }

        if (weaponIdleParent != null)
        {
            for (int i = weaponIdleParent.childCount - 1; i >= 0; i--)
                Destroy(weaponIdleParent.GetChild(i).gameObject);
        }

        if (rightHandTargetEmpty != null)
        {
            rightHandTargetEmpty.SetParent(transform);
            rightHandTargetEmpty.localPosition = Vector3.zero;
            rightHandTargetEmpty.localRotation = Quaternion.identity;
        }

        if (leftHandTargetEmpty != null)
        {
            leftHandTargetEmpty.SetParent(transform);
            leftHandTargetEmpty.localPosition = Vector3.zero;
            leftHandTargetEmpty.localRotation = Quaternion.identity;
        }
    }

    private void EnsureRigConstraints()
    {
        if (rigBuilder == null)
            rigBuilder = GetComponentInChildren<RigBuilder>();

        if (rigBuilder == null)
        {
            Debug.LogError("[WeaponIKController] RigBuilder not found! IK will not work.");
            return;
        }

        Rig rig = null;
        if (rigBuilder.layers != null && rigBuilder.layers.Count > 0)
            rig = rigBuilder.layers[0].rig;

        if (rig == null)
        {
            Debug.LogError("[WeaponIKController] Rig not found in RigBuilder!");
            return;
        }

        if (animator == null || !animator.isHuman)
        {
            Debug.LogError("[WeaponIKController] Humanoid Animator required for auto-creating IK.");
            return;
        }

        if (rightGrip == null)
        {
            rightGrip = FindOrCreateTwoBoneIK(
                rig.transform, "RightHandIK",
                HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
                rightHandTargetEmpty);
        }

        if (leftGrip == null)
        {
            leftGrip = FindOrCreateTwoBoneIK(
                rig.transform, "LeftHandIK",
                HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                leftHandTargetEmpty);
        }
    }

    private TwoBoneIKConstraint FindOrCreateTwoBoneIK(Transform parent, string name,
        HumanBodyBones upper, HumanBodyBones lower, HumanBodyBones hand,
        Transform target)
    {
        if (target == null)
            return null;

        var existing = parent.GetComponentsInChildren<TwoBoneIKConstraint>(true)
            .FirstOrDefault(c => c.name == name);
        if (existing != null)
            return existing;

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var constraint = go.AddComponent<TwoBoneIKConstraint>();
        constraint.weight = 0f;

        constraint.data.root = animator.GetBoneTransform(upper);
        constraint.data.mid = animator.GetBoneTransform(lower);
        constraint.data.tip = animator.GetBoneTransform(hand);
        constraint.data.target = target;

        return constraint;
    }
}
