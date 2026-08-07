using System;
using UnityEngine;

public class EquipmentStatsCalculator : BaseService, IEquipmentStatsService
{
    [Header("Формулы расчета")]
    [Tooltip("Бонус здоровья за единицу силы")]
    [SerializeField] private float healthPerStrength = 5f;
    [Tooltip("Базовая выносливость")]
    [SerializeField] private float baseStamina = 100f;
    [Tooltip("Бонус выносливости за единицу ловкости")]
    [SerializeField] private float staminaPerAgility = 2f;
    [Tooltip("Множитель скорости передвижения от ловкости")]
    [SerializeField] private float speedPerAgility = 0.02f;
    [Tooltip("Базовая регенерация выносливости")]
    [SerializeField] private float baseStaminaRegen = 5f;

    private IEquipmentService _equipment;
    private IPlayerProfileService _profileService;

    public FinalPlayerStats CurrentStats { get; private set; }
    public event Action<FinalPlayerStats> OnStatsChanged;

    protected override Type GetServiceType() => typeof(IEquipmentStatsService);

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();

        if (_equipment != null)
            _equipment.OnEquipmentChanged += OnEquipmentChanged;

        Recalculate();
    }

    private void OnDestroy()
    {
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= OnEquipmentChanged;
        base.OnDestroy();
    }

    private void OnEquipmentChanged(EquipmentSlotType slotType, ItemData item)
    {
        Recalculate();
    }

    public void Recalculate()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[EquipmentStatsCalculator] Профиль игрока не найден.");
            return;
        }

        var stats = new FinalPlayerStats();

        // Ролевые характеристики
        stats.Strength = profile.strength;
        stats.Intelligence = profile.intelligence;
        stats.Agility = profile.agility;

        // Механические характеристики из профиля
        stats.Level = profile.level;
        stats.MaxHealth = profile.maxHealth;
        stats.MaxMana = profile.maxMana;
        stats.MoveSpeed = profile.speedMove;
        stats.RunSpeed = profile.speedRun;
        stats.JumpForce = profile.jumpForce;
        stats.Acceleration = profile.acceleration;
        stats.Deceleration = profile.deceleration;
        stats.RotationSpeed = profile.rotationSpeed;
        stats.HealthRegenRate = profile.healthRegenRate;
        stats.ManaRegenRate = profile.manahRegenRate;

        // Производные формулы
        stats.MaxHealth += Mathf.RoundToInt(healthPerStrength * Mathf.Pow(stats.Strength, 0.8f));
        stats.MaxStamina = baseStamina + stats.Agility * staminaPerAgility;
        stats.MoveSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.RunSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.StaminaRegenRate = baseStaminaRegen + stats.Agility * 0.5f + stats.MaxStamina * 0.02f;

        // Бонусы от всей экипировки
        if (_equipment != null)
        {
            foreach (var slot in _equipment.Slots)
            {
                if (slot.Item == null)
                    continue;

                ApplyItemBonuses(stats, slot.Item);
            }

            // Активное оружие определяет боевые показатели
            var weapon = _equipment.CurrentWeapon;
            if (weapon != null && weapon.Stats != null)
            {
                stats.AttackDamage += weapon.Stats.damage;
                stats.AttackRange += weapon.Stats.range;
                stats.AttackDuration = weapon.Stats.attackDuration;
            }
        }

        CurrentStats = stats;
        OnStatsChanged?.Invoke(CurrentStats);
    }

    private void ApplyItemBonuses(FinalPlayerStats stats, ItemData item)
    {
        switch (item)
        {
            case ArmorData armor:
                stats.Armor += armor.armorValue;
                break;

            case TrinketData trinket:
                stats.MaxHealth += trinket.bonusHealth;
                break;
        }
    }
}
