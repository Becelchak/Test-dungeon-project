using UnityEngine;

public abstract class SkillEffectData : ScriptableObject
{
    public abstract void Apply(ICharacterStatsCalculator calc);
    public abstract void Remove(ICharacterStatsCalculator calc);
}

public enum GameEventType
{
    OnDamageTaken,
    OnKill,
}

public enum ModifierTarget
{
    // ─── StatKey (числовые статы) ───
    Strength, Intelligence, Agility,
    MaxHealth, MaxMana, MaxStamina, MaxSanity,
    CurrentHealth, CurrentMana, CurrentStamina, CurrentSanity,
    MoveSpeed, RunSpeed, JumpForce,
    Acceleration, Deceleration, RotationSpeed,
    HealthRegen, ManaRegen, StaminaRegen,
    OutgoingDamagePercent, IncomingDamagePercent,
    OutgoingDamageFlat, IncomingDamageFlat,
    Armor, BlockPercent, BlockFlat,
    ManaCostPercent, HealthCostPercent, StaminaCostPercent,
    GloryGainPercent,StaminaAttackCost,
    // ─── Статы касающиеся оружия ───
    OneHandDamage, TwoHandDamage,TrowDamage,
    LowStaminaOutgoingDamagePercent,
    AttackSpeed,

    // ─── CapabilityKey (открытие механик) ───
    Capability_Miracle,
    Capability_Parry,
    Capability_ParryDamageUp,
    Capability_InstantKill,
    Capability_Ritual_DamnatioMemoriae,
    Capability_Execute,
    Capability_TwoHandWeapon,
    Capability_TrowWeaponGrab,
    Capability_PerfectBlock,
    Capability_PerfectBlockDamageUp,
    Capability_CombatStyleChange,
}
