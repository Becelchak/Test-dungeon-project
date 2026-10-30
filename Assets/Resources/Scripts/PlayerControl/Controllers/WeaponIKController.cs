using UnityEngine.Animations.Rigging;
using UnityEngine;
using System.Linq;

public class WeaponIKController : MonoBehaviour
{
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform weaponHolder_R;
    [SerializeField] private Transform weaponHolder_L;
    [Tooltip("Parent for two-handed weapon in IDLE. Usually Hips/Spine/Root.")]
    [SerializeField] private Transform weaponIdleParent;
    [SerializeField] private TwoBoneIKConstraint rightGrip;
    [SerializeField] private TwoBoneIKConstraint leftGrip;
    [SerializeField] private Transform rightHandTargetEmpty;
    [SerializeField] private Transform leftHandTargetEmpty;

    [Header("IK Smoothing")]
    [SerializeField] private float weightLerpSpeed = 7f;
    [SerializeField] private float idleReturnDuration = 0.15f;

    [Header("Runtime")]
    [SerializeField] private bool useRightHandIK = false;

    public enum WeaponPose { Idle, Block, Parry, Attack }

    private IEquipmentService _equipment;
    private GameObject _currentWeaponObject;
    private WeaponData _currentWeaponData;
    private GameObject _currentShieldObject;
    private WeaponData _currentShieldData;

    public GameObject CurrentWeaponObject => _currentWeaponObject;
    public WeaponPose CurrentPose => _currentPose;
    public Animator PlayerAnimator => animator;

    public WeaponDamageSource CurrentWeaponDamageSource =>
        _currentWeaponObject != null
            ? _currentWeaponObject.GetComponentInChildren<WeaponDamageSource>(true)
            : null;

    private float _currentRightWeight;
    private float _currentLeftWeight;
    private float _targetRightWeight;
    private float _targetLeftWeight;

    private WeaponPose _currentPose = WeaponPose.Idle;
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

        if (animator == null) animator = GetComponentInChildren<Animator>();

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

        if (rightGrip != null) rightGrip.weight = _currentRightWeight;
        if (leftGrip != null) leftGrip.weight = _currentLeftWeight;
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
            if (rightGrip != null) rightGrip.weight = rightWeight;
            if (leftGrip != null) leftGrip.weight = leftWeight;
        }
    }

    public (float right, float left) GetTargetWeightsFor(WeaponHandling handling, bool useCombatPose)
    {
        return handling switch
        {
            WeaponHandling.RightHand => (useRightHandIK && !useCombatPose ? 1f : 0f, 0f),
            WeaponHandling.LeftHand => (0f, useRightHandIK && !useCombatPose ? 1f : 0f),
            WeaponHandling.BothHands => useCombatPose ? (0f, 0f) : (1f, 1f),
            WeaponHandling.OffHand => (0f, 0f),
            _ => (0f, 0f)
        };
    }

    public (float right, float left) GetTargetWeightsFor(WeaponHandling handling)
    {
        bool isBothHands = handling == WeaponHandling.BothHands;
        bool useCombatPose = _currentPose == WeaponPose.Attack
            || (isBothHands && (_currentPose == WeaponPose.Block || _currentPose == WeaponPose.Parry));
        return GetTargetWeightsFor(handling, useCombatPose);
    }

    public void SetPose(WeaponPose pose, bool instantly = false)
    {
        _currentPose = pose;
        ApplyCurrentPose(instantly);
    }

    private void ApplyCurrentPose(bool instantly = false)
    {
        if (_currentWeaponObject == null || _currentWeaponData == null) return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        bool isBothHands = _currentWeaponData.handling == WeaponHandling.BothHands;
        bool useCombatPose = _currentPose == WeaponPose.Attack
            || (isBothHands && (_currentPose == WeaponPose.Block || _currentPose == WeaponPose.Parry));

        if (isBothHands && weaponIdleParent != null)
        {
            Transform desiredParent = useCombatPose ? weaponHolder_R : weaponIdleParent;
            if (desiredParent != null && _currentWeaponObject.transform.parent != desiredParent)
                _currentWeaponObject.transform.SetParent(desiredParent, worldPositionStays: true);
        }

        ApplyWeaponTransform(_currentWeaponObject.transform, useCombatPose);

        if (rigBuilder != null) rigBuilder.Build();

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

    private void OnWeaponChanged(WeaponData weapon)
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        if (_currentWeaponObject != null) Destroy(_currentWeaponObject);
        if (_currentShieldObject != null) Destroy(_currentShieldObject);

        ClearHolders();
        _currentWeaponData = weapon;
        _currentShieldData = null;
        _currentPose = WeaponPose.Idle;

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
            Debug.LogWarning($"[WeaponIKController] Two-handed weapon '{weapon.name}' has no LeftGrip.");
            leftWeight = 0f;
        }

        SetIKWeights(rightWeight, leftWeight, true);
        RefreshShield();

        if (rigBuilder != null) rigBuilder.Build();
    }

    private void OnShieldChanged(WeaponData shield) => RefreshShield();

    private void RefreshShield()
    {
        if (_currentShieldObject != null)
        {
            Destroy(_currentShieldObject);
            _currentShieldObject = null;
            _currentShieldData = null;
        }

        var shield = _equipment?.CurrentShield;
        if (shield == null || shield.weaponPrefab == null) return;
        if (_currentWeaponData == null) return;
        if (_currentWeaponData.handling == WeaponHandling.BothHands) return;

        Transform shieldHolder = _currentWeaponData.handling == WeaponHandling.RightHand
            ? weaponHolder_L
            : weaponHolder_R;

        if (shieldHolder == null) return;

        _currentShieldData = shield;
        _currentShieldObject = Instantiate(shield.weaponPrefab, shieldHolder);
        _currentShieldObject.transform.localPosition = shield.weaponHolderOffset;
        _currentShieldObject.transform.localRotation = Quaternion.Euler(shield.weaponHolderRotationOffsetEuler);
    }

    private void AttachIKTarget(Transform target, Transform grip, Vector3 posOffset, Quaternion rotOffset)
    {
        if (target == null || grip == null) return;
        target.SetParent(grip);
        target.localPosition = posOffset;
        target.localRotation = rotOffset;
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
            for (int i = weaponHolder_R.childCount - 1; i >= 0; i--)
                Destroy(weaponHolder_R.GetChild(i).gameObject);

        if (weaponHolder_L != null)
            for (int i = weaponHolder_L.childCount - 1; i >= 0; i--)
                Destroy(weaponHolder_L.GetChild(i).gameObject);

        if (weaponIdleParent != null)
            for (int i = weaponIdleParent.childCount - 1; i >= 0; i--)
                Destroy(weaponIdleParent.GetChild(i).gameObject);

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
        if (rigBuilder == null) rigBuilder = GetComponentInChildren<RigBuilder>();
        if (rigBuilder == null) { Debug.LogError("[WeaponIKController] RigBuilder not found!"); return; }

        Rig rig = null;
        if (rigBuilder.layers != null && rigBuilder.layers.Count > 0)
            rig = rigBuilder.layers[0].rig;

        if (rig == null) { Debug.LogError("[WeaponIKController] Rig not found!"); return; }

        if (animator == null || !animator.isHuman)
        { Debug.LogError("[WeaponIKController] Humanoid Animator required."); return; }

        if (rightGrip == null)
            rightGrip = FindOrCreateTwoBoneIK(rig.transform, "RightHandIK",
                HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
                rightHandTargetEmpty);

        if (leftGrip == null)
            leftGrip = FindOrCreateTwoBoneIK(rig.transform, "LeftHandIK",
                HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                leftHandTargetEmpty);
    }

    private TwoBoneIKConstraint FindOrCreateTwoBoneIK(Transform parent, string name,
        HumanBodyBones upper, HumanBodyBones lower, HumanBodyBones hand, Transform target)
    {
        if (target == null) return null;

        var existing = parent.GetComponentsInChildren<TwoBoneIKConstraint>(true)
            .FirstOrDefault(c => c.name == name);
        if (existing != null) return existing;

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