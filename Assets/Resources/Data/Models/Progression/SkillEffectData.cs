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

public enum ModifierType
{
    Add,
    Multiply,
    Percent,
    Lock
}

public enum ActionType
{
    Cancel,
    Apply,
    Repeat
}

public enum StatType
{
    Strength,
    Intelligence,
    Agility,
    Health,
    Mana,
    Stamina,
    MoveSpeed,
    RunSpeed,
    JumpForce,
    Acceleration,
    Deceleration,
    RotationSpeed,
    HealthRegenRate,
    ManaRegenRate,
    StaminaRegenRate,
    AttackDamage,
    AttackRange,
    AttackDuration,
    Armor,
    AttackTake,
    BlockDamage
}
