using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FinalPlayerStats
{
    [Header("Ролевые характеристики")]
    public int Strength;
    public int Intelligence;
    public int Agility;
    public float Sanity;

    [Header("Механические характеристики")]
    public int Level;
    // Max
    public float MaxHealth;
    public float MaxMana;
    public float MaxStamina;
    public float MaxSanity;
    // Movement
    public float MoveSpeed;
    public float RunSpeed;
    public float JumpForce;
    public float Acceleration;
    public float Deceleration;
    public float RotationSpeed;

    [Header("Регенерация")]
    public float HealthRegenRate;
    public float ManaRegenRate;
    public float StaminaRegenRate;

    [Header("Боевая эффективность")]
    public float AttackDamage; //Базовый урон оружия (без модификаторов)
    public float AttackRange;
    public float AttackDuration;
    public float AttackSpeed;
    // Weapon
    public float OneHandDamage, TwoHandDamage, TrowDamage;
    public float LowStaminaOutgoingDamagePercent;
    // Defence
    public int Armor = 0;
    public float BlockPercent = 0;
    public int BlockFlat = 0;
    // Damage
    public float OutgoingDamagePercent = 0, IncomingDamagePercent = 0; // % наносимого и получаемого урона
    public float OutgoingDamageFlat = 0, IncomingDamageFlat = 0;
    //DamageType
    public float PierceDamageModifier = 0, SlashDamageModifier = 0, BluntDamageModifier = 0;
    // Costs
    public float ManaCostPercent, StaminaCostPercent;
    public float GloryGainPercent, StaminaAttackCost, StaminaDefenceCost;

    [Header("Специализированные механики")]
    public int Glory;
    public int MaxGlory;

    // ═══ Capabilities ═══
    public HashSet<ModifierTarget> Capabilities = new();

    public bool HasCapability(ModifierTarget cap) => Capabilities.Contains(cap);
}
