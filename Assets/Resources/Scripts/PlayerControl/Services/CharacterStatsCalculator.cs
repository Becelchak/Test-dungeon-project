using System;
using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<StatType, List<SkillEffectData>> _modifiers = new Dictionary<StatType, List<SkillEffectData>>();
    //private Dictionary<string, TriggeredEffect> _triggers = new Dictionary<string, TriggeredEffect>();
    private HashSet<string> _unlockedMechanics = new HashSet<string>();

    public FinalPlayerStats CurrentStats { get; private set; }
    public event Action<FinalPlayerStats> OnStatsChanged;

    protected override Type GetServiceType() => typeof(ICharacterStatsCalculator);

    private void Start()
    {
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        _progressionService = ServiceLocator.Instance.GetService<IProgressionService>();
        _triggerService = ServiceLocator.Instance.GetService<ITriggerService>();

        if (_equipment != null)
            _equipment.OnEquipmentChanged += OnEquipmentChanged;

        if (_progressionService != null)
        {
            _progressionService.OnLevelUp += OnLevelUp;
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
        {
            _progressionService.OnLevelUp -= OnLevelUp;
        }

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

    public void AddModifier(StatType stat, SkillEffectData source)
    {
        if (!_modifiers.ContainsKey(stat))
            _modifiers[stat] = new List<SkillEffectData>();

        if (!_modifiers[stat].Contains(source))
        {
            _modifiers[stat].Add(source);
            Recalculate();
        }
    }

    public void RemoveModifier(StatType stat, SkillEffectData source)
    {
        if (_modifiers.ContainsKey(stat))
        {
            _modifiers[stat].Remove(source);
            Recalculate();
        }
    }

    public void UnlockMechanic(string mechanicId)
    {
        if (_unlockedMechanics.Add(mechanicId))
        {
            Debug.Log($"[CharacterStatsCalculator] Mechanic unlocked: {mechanicId}");
        }
    }

    public void LockMechanic(string mechanicId)
    {
        _unlockedMechanics.Remove(mechanicId);
    }

    public bool HasMechanic(string mechanicId)
    {
        return _unlockedMechanics.Contains(mechanicId);
    }

    public void RegisterTrigger(TriggeredEffect trigger)
    {
        //if(!_triggers.ContainsKey(trigger.triggerId))
        //{
        //    _triggers.Add(trigger.triggerId, trigger);
        //    Debug.Log($"Зарегестрирован триггер {trigger.triggerId}");
        //}
        //else
        //    Debug.Log($"Триггер {trigger.triggerId} уже зарегестрирован!");
        _triggerService.Register(trigger);
    }

    public void UnregisterTrigger(TriggeredEffect trigger)
    {
        //if (_triggers.ContainsKey(trigger.triggerId))
        //{
        //    _triggers.Remove(trigger.triggerId);
        //    Debug.Log($"Удален триггер {trigger.triggerId}");
        //}
        //else
        //    Debug.Log($"Триггер {trigger.triggerId} не найден!");
        _triggerService.Unregister(trigger);
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

        stats.MaxHealth += Mathf.RoundToInt(healthPerStrength * Mathf.Pow(stats.Strength, 0.8f));
        stats.MaxStamina = baseStamina + stats.Agility * staminaPerAgility;
        stats.MoveSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.RunSpeed *= 1f + stats.Agility * speedPerAgility;
        stats.StaminaRegenRate = baseStaminaRegen + stats.Agility * 0.5f + stats.MaxStamina * 0.02f;

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

        ApplySkillModifiers(stats);
        ApplySpecialMechanics(profile, stats);

        CurrentStats = stats;
        OnStatsChanged?.Invoke(CurrentStats);
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

    private void ApplySkillModifiers(FinalPlayerStats stats)
    {
        foreach (var kvp in _modifiers)
        {
            foreach (var effect in kvp.Value)
            {
                if (effect is StatModifierEffect mod)
                {
                    ApplyStatModifier(stats, mod);
                }
            }
        }
    }

    private void ApplyStatModifier(FinalPlayerStats stats, StatModifierEffect mod)
    {
        switch (mod.stat)
        {
            case StatType.Strength:
                ApplyToStat(ref stats.Strength, mod);
                break;
            case StatType.Intelligence:
                ApplyToStat(ref stats.Intelligence, mod);
                break;
            case StatType.Agility:
                ApplyToStat(ref stats.Agility, mod);
                break;
            case StatType.Health:
                ApplyToStat(ref stats.MaxHealth, mod);
                break;
            case StatType.Mana:
                ApplyToStat(ref stats.MaxMana, mod);
                break;
            case StatType.Stamina:
                ApplyToStat(ref stats.MaxStamina, mod);
                break;
            case StatType.MoveSpeed:
                ApplyToStat(ref stats.MoveSpeed, mod);
                break;
            case StatType.RunSpeed:
                ApplyToStat(ref stats.RunSpeed, mod);
                break;
            case StatType.JumpForce:
                ApplyToStat(ref stats.JumpForce, mod);
                break;
            case StatType.Acceleration:
                ApplyToStat(ref stats.Acceleration, mod);
                break;
            case StatType.Deceleration:
                ApplyToStat(ref stats.Deceleration, mod);
                break;
            case StatType.RotationSpeed:
                ApplyToStat(ref stats.RotationSpeed, mod);
                break;
            case StatType.HealthRegenRate:
                ApplyToStat(ref stats.HealthRegenRate, mod);
                break;
            case StatType.ManaRegenRate:
                ApplyToStat(ref stats.ManaRegenRate, mod);
                break;
            case StatType.StaminaRegenRate:
                ApplyToStat(ref stats.StaminaRegenRate, mod);
                break;
            case StatType.AttackDamage:
                ApplyToStat(ref stats.AttackDamage, mod);
                break;
            case StatType.AttackRange:
                ApplyToStat(ref stats.AttackRange, mod);
                break;
            case StatType.AttackDuration:
                ApplyToStat(ref stats.AttackDuration, mod);
                break;
            case StatType.Armor:
                ApplyToStat(ref stats.Armor, mod);
                break;
        }
    }

    private void ApplyToStat(ref int stat, StatModifierEffect mod)
    {
        switch (mod.type)
        {
            case ModifierType.Add:
                stat += Mathf.RoundToInt(mod.value);
                break;
            case ModifierType.Multiply:
                stat *= Mathf.RoundToInt(mod.value);
                break;
            case ModifierType.Percent:
                stat = Mathf.RoundToInt(stat * (1f + mod.value / 100f));
                break;
        }
    }

    private void ApplyToStat(ref float stat, StatModifierEffect mod)
    {
        switch (mod.type)
        {
            case ModifierType.Add:
                stat += mod.value;
                break;
            case ModifierType.Multiply:
                stat *= mod.value;
                break;
            case ModifierType.Percent:
                stat *= (1f + mod.value / 100f);
                break;
        }
    }
}
