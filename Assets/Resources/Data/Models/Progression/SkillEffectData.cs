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
    MoveSpeed, RunSpeed, JumpForce,
    Acceleration, Deceleration, RotationSpeed,
    HealthRegen, ManaRegen, StaminaRegen,
    OutgoingDamagePercent, IncomingDamagePercent,
    Armor, BlockPercent, BlockFlat,
    ManaCostPercent, HealthCostPercent, StaminaCostPercent,
    GloryGainPercent,

    // ─── CapabilityKey (открытие механик) ───
    Capability_Miracle,
    Capability_Parry,
    Capability_InstantKill,
    Capability_Ritual_DamnatioMemoriae,
    Capability_Execute,
}
