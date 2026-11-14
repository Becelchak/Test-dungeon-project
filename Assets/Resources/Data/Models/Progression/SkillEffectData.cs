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
    // ═══════════════════════════════════════════════════════════════════
    // STAT BLOCK — 1..249
    // ═══════════════════════════════════════════════════════════════════

    // ─── Primary stats (1..24)───
    Strength = 1,
    Intelligence = 2,
    Agility = 3,
    Sanity = 4,
    // 5..24 → reserve

    // ─── Max stats (25..49)───
    MaxHealth = 25,
    MaxMana = 26,
    MaxStamina = 27,
    MaxSanity = 28,
    // 29..49 → reserve

    // ─── Current stats (50..74) ───
    CurrentHealth = 50,
    CurrentMana = 51,
    CurrentStamina = 52,
    CurrentSanity = 53,
    // 54..74 → reserve

    // ─── Movement (75..99) ───
    MoveSpeed = 75,
    RunSpeed = 76,
    JumpForce = 77,
    Acceleration = 78,
    Deceleration = 79,
    RotationSpeed = 80,
    // 81..99 → reserve

    // ─── Regen (100..124) ───
    HealthRegen = 100,
    ManaRegen = 101,
    StaminaRegen = 102,
    // 103..124 → reserve

    // ─── Damage modifiers (125..149) ───
    OutgoingDamagePercent = 125,
    IncomingDamagePercent = 126,
    OutgoingDamageFlat = 127,
    IncomingDamageFlat = 128,
    // 129..149 → reserve

    // ─── Defense (150..174) ───
    Armor = 150,
    BlockPercent = 151,
    BlockFlat = 152,
    LimbDamageModifier = 153,
    // 153..174 → reserve

    // ─── Cost modifiers (175..199) ───
    ManaCostPercent = 175,
    HealthCostPercent = 176,
    StaminaCostPercent = 177,
    // 178..199 → reserve

    // ─── Glory / stamina costs (200..224) ───
    GloryGainPercent = 200,
    StaminaAttackCost = 201,
    StaminaDefenceCost = 202,
    // 203..224 → reserve

    // 225..249 → резерв под новые sub-блоки статов

    // ═══════════════════════════════════════════════════════════════════
    // WEAPON BLOCK — 250..349
    // ═══════════════════════════════════════════════════════════════════

    OneHandDamage = 250,
    TwoHandDamage = 251,
    TrowDamage = 252,
    LowStaminaOutgoingDamagePercent = 253,
    AttackSpeed = 254,
    PierceDamageModifier = 255,
    SlashDamageModifier = 256,
    BluntDamageModifier = 257,
    // 255..349 → reserve

    // ═══════════════════════════════════════════════════════════════════
    // CAPABILITY BLOCK — 350..449
    // ═══════════════════════════════════════════════════════════════════

    Capability_Miracle = 350,
    Capability_Parry = 351,
    Capability_ParryDamageUp = 352,
    Capability_InstantKill = 353,
    Capability_Ritual_DamnatioMemoriae = 354,
    Capability_Execute = 355,
    Capability_TwoHandWeapon = 356,
    Capability_TrowWeaponGrab = 357,
    Capability_PerfectBlock = 358,
    Capability_PerfectBlockDamageUp = 359,
    Capability_CombatStyleChange = 360,
    Capability_MultyTargetDamage = 361,
    // 361..449 → reserve 
}
