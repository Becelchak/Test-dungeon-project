using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovementService movement;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    private PlayerProfileService playerProfile;
    private CharacterRotator characterRotator;

    private IEquipmentService equipment;
    private WeaponData currentWeapon;
    private AnimatorOverrideController overrideController;

    private bool isRunning;

    private IEquipmentStatsService _equipmentStatsService;

    private void Start()
    {
        playerProfile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _equipmentStatsService = ServiceLocator.Instance.GetService<IEquipmentStatsService>();

        var stats = _equipmentStatsService?.CurrentStats;
        walkSpeed = stats?.MoveSpeed ?? playerProfile.CurrentProfile.speedMove;
        runSpeed = stats?.RunSpeed ?? playerProfile.CurrentProfile.speedRun;

        characterRotator = GetComponent<CharacterRotator>();
        equipment = (EquipmentService) ServiceLocator.Instance.GetService<IEquipmentService>();
        equipment.OnWeaponChanged += OnWeaponChanged;
        OnWeaponChanged(equipment.CurrentWeapon);
    }

    private void OnDestroy()
    {
        if (equipment != null)
            equipment.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(WeaponData weapon)
    {
        overrideController = animator.runtimeAnimatorController as AnimatorOverrideController ;
        if (overrideController == null)
        {
            // Если это обычный контроллер, нужно создать обертку-оверрайд
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        // TODO: Вынести в отдельный метод замену базовых анимаций оружия
        TriggerRandomAttack(weapon);

        currentWeapon = weapon;
        animator.SetFloat("WeaponType", (int)weapon.weaponType);
        Debug.Log($"Текущее оружие {currentWeapon}");
    }

    public void TriggerRandomAttack(WeaponData weapon)
    {
        if (weapon == null || weapon.attackAnimationClips == null || weapon.attackAnimationClips.Count == 0)
        {
            Debug.LogWarning($"[PlayerAnimationController] У оружия нет анимаций атаки: {weapon?.name}");
            return;
        }

        int randomIndex = Random.Range(0, weapon.attackAnimationClips.Count);
        overrideController["Base_Attack"] = weapon.attackAnimationClips[randomIndex];
    }

    public float GetCurrentAttackClipLength()
    {
        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (overrideController != null)
        {
            AnimationClip clip = overrideController["Base_Attack"];
            if (clip != null) return clip.length; // Возвращает длину файла в секундах
        }
        return 1f;
    }

    /// <summary>
    /// Включает или выключает анимацию блока.
    /// </summary>
    public void SetBlocking(bool isBlocking)
    {
        if (animator != null)
            animator.SetBool("Block", isBlocking);
    }

    /// <summary>
    /// Проигрывает триггер анимации подбора предмета.
    /// </summary>
    public void TriggerPickup()
    {
        if (animator != null)
            animator.SetTrigger("Pickup");
    }

    private void FixedUpdate()
    {
        isRunning = movement._currentSpeed > walkSpeed;
        Vector2 localInput = movement.GetLocalMovementInput(characterRotator.rotationModel);
        float maxSpeed = isRunning ? runSpeed : walkSpeed;
        float forward = Mathf.Clamp(localInput.y / maxSpeed, -1f, 1f);
        float right = Mathf.Clamp(localInput.x / maxSpeed, -1f, 1f);

        animator.SetFloat("ForwardVelocity", forward);
        animator.SetFloat("RightVelocity", right);
        animator.SetFloat("Speed", Mathf.Clamp01(movement._currentSpeed / maxSpeed));
        //Debug.Log($"_currentSpeed: {movement._currentSpeed}, maxSpeed: {maxSpeed}");
    }
}