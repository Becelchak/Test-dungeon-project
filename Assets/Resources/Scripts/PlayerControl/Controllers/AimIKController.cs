using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator))]
public class AimIKController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponIKController weaponIK;
    [SerializeField] private CharacterRotator characterRotator;

    [Header("IK Settings")]
    [SerializeField] private float weightLerpSpeed = 8f;

    [Header("Left Hand IK (Idle only)")]
    [Range(0f, 1f)]
    [Tooltip("IK левой руки к рукояти для двуручного ВНЕ атаки. " +
             "В атаке левой управляет thrust-impact IK (если Thrust).")]
    [SerializeField] private float leftHandWeight = 1f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;

    private Animator _animator;
    private IEquipmentService _equipment;
    private IAimingService _aiming;

    private WeaponData _currentWeapon;
    private AttackDefinition _currentAttack;
    [Tooltip("Вес той или иной атаки и влияния ее на исходную анимацию")]
    private float _currentWeight;
    private float _targetWeight;
    [Tooltip("Прогресс анимации атаки из состояния атаки PlayerAttackState")]
    private float _attackProgress;

    [Tooltip("Единый объект прицеливания")]
    private Transform _weaponAimTarget;

    public Transform WeaponAimTarget => _weaponAimTarget;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        if (weaponIK == null) weaponIK = GetComponent<WeaponIKController>();
        if (characterRotator == null) characterRotator = GetComponent<CharacterRotator>();

        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _aiming = ServiceLocator.Instance.GetService<IAimingService>();

        // Найти weaponAimTarget в Rig, если не назначен в редакторе
        var rigBuilder = GetComponentInChildren<RigBuilder>();
        if (rigBuilder != null && rigBuilder.layers != null && rigBuilder.layers.Count > 0)
        {
            var rig = rigBuilder.layers[0].rig;
            if (rig != null)
            {
                _weaponAimTarget = rig.transform.Find("weaponAimTarget");
                if (_weaponAimTarget == null)
                {
                    Debug.LogWarning("[AimIKController] weaponAimTarget не найден в Rig!");
                }
                else
                {
                    Debug.Log("[AimIKController] weaponAimTarget найден и подключен.");
                }
            }
        }

        if (_equipment != null)
        {
            _equipment.OnWeaponChanged += OnWeaponChanged;
            OnWeaponChanged(_equipment.CurrentWeapon);
        }
    }

    private void OnDestroy()
    {
        if (_equipment != null) _equipment.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(WeaponData weapon)
    {
        _currentWeapon = weapon;
        _currentAttack = null;
        if (weapon == null || weapon.weaponPrefab == null) _targetWeight = 0f;
    }

    /// <summary>Включить/выключить слой атаки. Вызывать из PlayerAttackState.</summary>
    public void SetAimActive(bool active)
    {
        if (_currentWeapon == null || _currentWeapon.weaponPrefab == null) return;
        _targetWeight = active ? 1f : 0f;
    }

    /// <summary>Задать активную атаку (Slash/Thrust). null — сброс.</summary>
    public void SetAttackMode(AttackDefinition attack) => _currentAttack = attack;

    /// <summary>Передаётся из PlayerAttackState.Update каждый кадр: _attackTimer / _attackDuration.</summary>
    public void SetAttackProgress(float normalizedProgress)
    {
        _attackProgress = Mathf.Clamp01(normalizedProgress);
    }

    private void LateUpdate()
    {
        _currentWeight = Mathf.MoveTowards(_currentWeight, _targetWeight, weightLerpSpeed * Time.deltaTime);

        // Обновляем позицию weaponAimTarget из AimingService
        if (_weaponAimTarget != null && _aiming != null)
        {
            _weaponAimTarget.position = _aiming.AimPoint;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_currentWeapon == null || _aiming == null) return;

        var weaponObj = weaponIK != null ? weaponIK.CurrentWeaponObject : null;
        if (weaponObj == null) return;

        bool isAttacking = weaponIK != null
            && weaponIK.CurrentPose == WeaponIKController.WeaponPose.Attack;

        // ─── Правая рука: всегда отключена для AvatarIK ───
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);

        // ─── Корпус: Slash и Thrust ───
        if (_currentAttack != null && isAttacking)
        {
            if (_currentAttack.type == MeleeAttackType.Slash)
                ApplySlashBodyAim();
            else if (_currentAttack.type == MeleeAttackType.Thrust)
                ApplyThrustBodyAim();
        }

        // ─── Левая рука в Idle (TwoHanded) ───
        if (_currentWeapon.handling == WeaponHandling.BothHands
            && !isAttacking && leftHandWeight > 0.01f && _currentWeight > 0.01f)
        {
            Transform leftGrip = weaponObj.transform.Find("LeftGrip");
            if (leftGrip != null)
            {
                float w = _currentWeight * leftHandWeight;
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, w);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, w);
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftGrip.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, leftGrip.rotation);
            }
        }
    }

    /// <summary>
    /// Процедурный поворот торса для режущих атак
    /// </summary>
    private void ApplySlashBodyAim()
    {
        ApplyBodyAim(isSlash: true);
    }

    /// <summary>
    /// Процедурный поворот торса для колющих атак
    /// </summary>
    private void ApplyThrustBodyAim()
    {
        ApplyBodyAim(isSlash: false);
    }

    /// <summary>
    /// Общий метод поворота торса для Slash и Thrust
    /// </summary>
    /// <param name="isSlash"></param>
    private void ApplyBodyAim(bool isSlash)
    {
        if (_currentAttack == null) return;

        Vector3 aimPoint = _weaponAimTarget != null ? _weaponAimTarget.position : _aiming.AimPoint;
        Transform chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
        if (chest == null) return;

        Vector3 toAim = aimPoint - chest.position;
        Vector3 flat = toAim; flat.y = 0f;
        float horizontalDist = flat.magnitude;
        if (horizontalDist < 0.3f) return; // слишком близко

        // pitch > 0 → цель выше; pitch < 0 → цель ниже
        float pitchDeg = Mathf.Atan2(toAim.y, horizontalDist) * Mathf.Rad2Deg;
        pitchDeg = Mathf.Clamp(pitchDeg, -50f, 50f);

        // Ось — горизонтальный «right» персонажа (перпендикуляр к model.forward)
        Vector3 modelFwd = characterRotator != null ? characterRotator.rotationModel.forward : transform.forward;
        Vector3 rightAxis = Vector3.Cross(Vector3.up, modelFwd);
        if (rightAxis.sqrMagnitude < 0.001f) rightAxis = transform.right;
        rightAxis.Normalize();

        // Вес: зависит от типа атаки
        float w = _currentWeight * _currentAttack.slashBodyAimWeight;
        if (isSlash)
        {
            // Slash: WindowRamp для ramp in/out
            w *= WindowRamp(_attackProgress, 0.1f, 0.5f); // ramp in 0.1..0.5
            w *= 1f - WindowRamp(_attackProgress, 0.8f, 0.95f); // ramp out 0.8..0.95
        }
        else
        {
            // Thrust: ComputeThrustWeight для окна импакта
            w *= ComputeThrustWeight(_currentAttack, _attackProgress);
        }
        
        if (w < 0.01f) return;

        // Отрисовка для отладки
        if (drawDebug)
        {
            Color lineColor = isSlash ? Color.red : Color.orange;
            Color crossColor = isSlash ? Color.yellow : Color.orange;
            string logPrefix = isSlash ? "SlashBodyAim" : "ThrustBodyAim";
            
            // Линия от груди к AimPoint
            Debug.DrawLine(chest.position, aimPoint, lineColor);
            // Линия от груди по горизонтали
            Vector3 flatAim = chest.position + flat.normalized * horizontalDist;
            Debug.DrawLine(chest.position, flatAim, Color.green);
            // Линия вертикального угла
            Debug.DrawRay(flatAim, Vector3.up * toAim.y, Color.blue);
            // Сам AimPoint
            Debug.DrawRay(aimPoint, Vector3.up * 0.5f, crossColor);
            // Крест на AimPoint для чёткости
            Debug.DrawRay(aimPoint + Vector3.left * 0.3f, Vector3.right * 0.6f, crossColor);
            Debug.DrawRay(aimPoint + Vector3.back * 0.3f, Vector3.forward * 0.6f, crossColor);
            
            Debug.Log($"[{logPrefix}] Pitch: {pitchDeg:F2}°, HorizDist: {horizontalDist:F2}m, ToAimY: {toAim.y:F2}m, Weight: {w:F2}, AimPoint: {aimPoint}");
        }

        // Распределить наклон по цепочке костей
        ApplyWorldPitchDelta(HumanBodyBones.Spine, pitchDeg * 0.5f, rightAxis, w);
        ApplyWorldPitchDelta(HumanBodyBones.Chest, pitchDeg * 0.4f, rightAxis, w);
        ApplyWorldPitchDelta(HumanBodyBones.UpperChest, pitchDeg * 0.3f, rightAxis, w);
    }

    /// <summary>Поворот кости вокруг МИРОВОЙ оси на angleDeg, с блендом по weight.</summary>
    private void ApplyWorldPitchDelta(HumanBodyBones bone, float angleDeg, Vector3 worldAxis, float weight)
    {
        if (Mathf.Abs(angleDeg) < 0.05f || weight < 0.01f) return;
        Transform boneT = _animator.GetBoneTransform(bone);
        if (boneT == null || boneT.parent == null) return;

        // Знак: pitch>0 (цель выше) → наклонять корпус НАЗАД (смотреть вверх).
        // AngleAxis(θ, right) при право-рукой системе вращает forward вниз при θ>0,
        Quaternion delta = Quaternion.AngleAxis(-angleDeg, worldAxis);

        Quaternion worldCurrent = boneT.rotation;
        Quaternion worldNew = delta * worldCurrent;
        Quaternion worldBlend = Quaternion.Slerp(worldCurrent, worldNew, weight);

        Quaternion parentWorld = boneT.parent.rotation;
        Quaternion localNew = Quaternion.Inverse(parentWorld) * worldBlend;
        _animator.SetBoneLocalRotation(bone, localNew);
    }

    /// <summary>
    /// Вес IK для Thrust: 0 → ramp in → 1 (impact) → ramp out → 0.
    /// Замах и восстановление полностью отдаются анимации.
    /// </summary>
    private float ComputeThrustWeight(AttackDefinition a, float t)
    {
        float start = a.thrustImpactStart;
        float end = a.thrustImpactEnd;
        float ramp = Mathf.Max(0.02f, a.thrustWeightRamp);

        if (t < start - ramp) return 0f;
        if (t < start)
            return Mathf.SmoothStep(0f, 1f, (t - (start - ramp)) / ramp);
        if (t < end) return 1f;
        if (t < end + ramp)
            return Mathf.SmoothStep(1f, 0f, (t - end) / ramp);
        return 0f;
    }

    /// <summary>Плавное окно: 0 → 1 на [start, start+rampIn], 1 после.</summary>
    private float WindowRamp(float t, float start, float end)
    {
        if (t < start) return 0f;
        if (t < end) return Mathf.SmoothStep(0f, 1f, (t - start) / (end - start));
        return 1f;
    }
}