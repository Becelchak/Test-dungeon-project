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

    private IEquipmentService _equipment;
    private GameObject _currentWeaponObject;
    private WeaponData _currentWeaponData;
    private GameObject _currentShieldObject;
    private WeaponData _currentShieldData;

    private float _currentRightWeight;
    private float _currentLeftWeight;
    private float _targetRightWeight;
    private float _targetLeftWeight;

    private bool _isAttacking;
    private bool _isBlocking;
    private Coroutine _transitionCoroutine;

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

        EnsureRigConstraints();

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

    public (float right, float left) GetTargetWeightsFor(WeaponHandling handling)
    {
        return handling switch
        {
            WeaponHandling.RightHand => (useRightHandIK ? 1f : 0f, 0f),
            WeaponHandling.LeftHand => (0f, useRightHandIK ? 1f : 0f),
            WeaponHandling.BothHands => _isAttacking ? (0f, 0.285f) : (1f, 1f),
            WeaponHandling.OffHand => (0f, 0f),
            _ => (0f, 0f)
        };
    }

    public void SetBLockMode(bool isBlocking, bool instantly = false)
    {
        if (_currentWeaponData == null || _currentWeaponData.handling != WeaponHandling.BothHands)
            return;

        if (weaponIdleParent == null)
            return;

        _isBlocking = isBlocking;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        Transform newParent = isBlocking ? weaponHolder_R : weaponIdleParent;
        if (_currentWeaponObject != null && newParent != null)
        {
            _currentWeaponObject.transform.SetParent(newParent, worldPositionStays: true);
            ApplyWeaponTransform(_currentWeaponObject.transform, isBlocking);
        }

        // Перестраиваем граф Rig, т.к. цели IK сменили родителя
        if (rigBuilder != null)
            rigBuilder.Build();

        var (rightWeight, leftWeight) = GetTargetWeightsFor(WeaponHandling.BothHands);
        SetIKWeights(rightWeight, leftWeight, instantly);

        if (!isBlocking && _currentWeaponObject != null)
        {
            _transitionCoroutine = StartCoroutine(TransitionToIdlePose());
        }
    }

    public void SetAttackMode(bool isAttacking, bool instantly = false)
    {
        if (_currentWeaponData == null || _currentWeaponData.handling == WeaponHandling.OffHand)
            return;

        _isAttacking = isAttacking;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        if (_currentWeaponObject == null)
            return;

        // Для двуручного оружия меняем родителя между IDLE-родителем и правой рукой
        if (_currentWeaponData.handling == WeaponHandling.BothHands && weaponIdleParent != null)
        {
            Transform newParent = isAttacking ? weaponHolder_R : weaponIdleParent;
            _currentWeaponObject.transform.SetParent(newParent, worldPositionStays: true);
        }

        ApplyWeaponTransform(_currentWeaponObject.transform, isAttacking);

        // Перестраиваем граф Rig, т.к. цели IK сменили родителя
        if (rigBuilder != null)
            rigBuilder.Build();

        var (rightWeight, leftWeight) = GetTargetWeightsFor(_currentWeaponData.handling);
        SetIKWeights(rightWeight, leftWeight, instantly);

        if (!isAttacking)
        {
            _transitionCoroutine = StartCoroutine(TransitionToIdlePose());
        }
    }

    private void ApplyWeaponTransform(Transform weaponTransform, bool isAttacking)
    {
        if (weaponTransform == null || _currentWeaponData == null) return;

        Vector3 targetPos = isAttacking
            ? _currentWeaponData.weaponHolderAttackOffset
            : _currentWeaponData.weaponHolderOffset;
        Quaternion targetRot = Quaternion.Euler(isAttacking
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
        _isAttacking = false;

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
