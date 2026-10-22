using System;
using System.Collections.Generic;
using UnityEngine;
using EventBusSystem;

public class CharacterStatsCalculator : BaseService, ICharacterStatsCalculator
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

    private IPlayerProfileService _profileService;
    private IEquipmentService _equipment;
    private IProgressionService _progressionService;
    private ITriggerService _triggerService;
    private IModifierService _modifierService;

    public FinalPlayerStats CurrentStats { get; private set; }
    public event Action<FinalPlayerStats> OnStatsChanged;

    protected override Type GetServiceType() => typeof(ICharacterStatsCalculator);
    private void Start()
    {
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _progressionService = ServiceLocator.Instance.GetService<IProgressionService>();
        _triggerService = ServiceLocator.Instance.GetService<ITriggerService>();
        _modifierService = ServiceLocator.Instance.GetService<IModifierService>();

        if (_equipment != null)
            _equipment.OnEquipmentChanged += OnEquipmentChanged;

        if (_progressionService != null)
        {
            _progressionService.OnLevelUp += OnLevelUp;
        }

        if (_modifierService != null)
        {
            _modifierService.OnModifiersChanged += Recalculate;
        }

        if (_profileService != null)
            _profileService.OnProfileChanged += Recalculate;

        Recalculate();
    }

    private void OnDestroy()
    {
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= OnEquipmentChanged;

        if (_progressionService != null)
            _progressionService.OnLevelUp -= OnLevelUp;

        if (_modifierService != null)
            _modifierService.OnModifiersChanged -= Recalculate;

        if (_profileService != null)
            _profileService.OnProfileChanged -= Recalculate;
    }

    private void OnLevelUp(int newLevel)
    {
        Recalculate();
    }

    private void OnEquipmentChanged(EquipmentSlotType slotType, ItemData item)
    {
        Recalculate();
    }

    public List<SkillEffectData> GetEffectsFor(string sourceId)
    {
        var skills = _progressionService.AllSkillNodes;
        var traits = _progressionService.AllTraits;
        var effects = new List<SkillEffectData>();
        if (skills.ContainsKey(sourceId))
            effects.AddRange(skills[sourceId].effects);
        if (traits.ContainsKey(sourceId))
            effects.AddRange(traits[sourceId].effects);
        return effects;
    }
    public void RegisterTrigger(TriggeredEffect trigger)
    {
        _triggerService.Register(trigger);
    }

    public void UnregisterTrigger(TriggeredEffect trigger)
    {
        _triggerService.Unregister(trigger);
    }

    public void SyncCurrentHealthWithMax()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null) return;

        var maxHealth = CurrentStats.MaxHealth;
        if (profile.health != maxHealth)
        {
            profile.health = maxHealth;
            _profileService.SaveProfile(profile);
            Debug.Log($"[CharacterStatsCalculator] Текущее здоровье синхронизировано с максимальным: {maxHealth}");
        }
    }

    public void Recalculate()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Debug.LogWarning("[CharacterStatsCalculator] Profile not found");
            return;
        }

        var stats = new FinalPlayerStats();

        // Базовые статы из профиля
        stats.Strength = profile.strength;
        stats.Intelligence = profile.intelligence;
        stats.Agility = profile.agility;

        stats.Level = profile.currentLevel;
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

        // Базовые формулы
        stats.MaxHealth += Mathf.RoundToInt(healthPerStrength * Mathf.Pow(stats.Strength, 0.8f));
        stats.MaxStamina = baseStamina + stats.Agility * staminaPerAgility;
        stats.MoveSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.RunSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.StaminaRegenRate = baseStaminaRegen + stats.Agility * 0.5f + stats.MaxStamina * 0.02f;

        // Бонусы от экипировки
        if (_equipment != null)
        {
            foreach (var slot in _equipment.Slots)
            {
                if (slot.Item == null) continue;
                ApplyItemBonuses(stats, slot.Item);
            }

            var weapon = _equipment.CurrentWeapon;
            if (weapon != null && weapon.Stats != null)
            {
                stats.AttackDamage += weapon.Stats.damage;
                stats.AttackRange += weapon.Stats.range;
                stats.AttackDuration = weapon.Stats.attackDuration;
            }
        }

        // Применяем модификаторы из ModifierService
        ApplyModifierServiceModifiers(stats);

        // Специальные механики (Слава и т.д.)
        ApplySpecialMechanics(profile, stats);

        CurrentStats = stats;
        OnStatsChanged?.Invoke(CurrentStats);
        EventBus.RaiseEvent<IStatsChangedEventSubscriber>(
            s => s.OnStatsChanged(new StatsChangedEvent(stats))
        );
    }

    private void ApplyModifierServiceModifiers(FinalPlayerStats stats)
    {
        if (_modifierService == null) return;

        // Применяем модификаторы ко всем статам
        stats.Strength = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.Strength, stats.Strength));
        stats.Intelligence = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.Intelligence, stats.Intelligence));
        stats.Agility = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.Agility, stats.Agility));
        stats.MaxHealth = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.MaxHealth, stats.MaxHealth));
        stats.MaxMana = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.MaxMana, stats.MaxMana));
        stats.MaxStamina = _modifierService.GetValue(ModifierTarget.MaxStamina, stats.MaxStamina);
        stats.MoveSpeed = _modifierService.GetValue(ModifierTarget.MoveSpeed, stats.MoveSpeed);
        stats.RunSpeed = _modifierService.GetValue(ModifierTarget.RunSpeed, stats.RunSpeed);
        stats.JumpForce = _modifierService.GetValue(ModifierTarget.JumpForce, stats.JumpForce);
        stats.Acceleration = _modifierService.GetValue(ModifierTarget.Acceleration, stats.Acceleration);
        stats.Deceleration = _modifierService.GetValue(ModifierTarget.Deceleration, stats.Deceleration);
        stats.RotationSpeed = _modifierService.GetValue(ModifierTarget.RotationSpeed, stats.RotationSpeed);
        stats.HealthRegenRate = _modifierService.GetValue(ModifierTarget.HealthRegen, stats.HealthRegenRate);
        stats.ManaRegenRate = _modifierService.GetValue(ModifierTarget.ManaRegen, stats.ManaRegenRate);
        stats.AttackDamage = _modifierService.GetValue(ModifierTarget.OutgoingDamagePercent, stats.AttackDamage);
        stats.Armor = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.Armor, stats.Armor));
        stats.BlockPercent = _modifierService.GetValue(ModifierTarget.BlockPercent, stats.BlockPercent);
        stats.BlockFlat = Mathf.RoundToInt(_modifierService.GetValue(ModifierTarget.BlockFlat, stats.BlockFlat));
    }

    private void ApplySpecialMechanics(PlayerProfile profile, FinalPlayerStats stats)
    {
        if (profile.specialMechanics == null) return;

        foreach (var mechanic in profile.specialMechanics)
        {
            if (mechanic.mechanicId == "glory" && mechanic.isActive)
            {
                stats.Glory = mechanic.currentValue;
                stats.MaxGlory = 100;
            }
        }
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
