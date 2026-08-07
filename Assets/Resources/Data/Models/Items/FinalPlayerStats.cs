using System;
using UnityEngine;

[Serializable]
public class FinalPlayerStats
{
    [Header("Ролевые характеристики")]
    public int Strength;
    public int Intelligence;
    public int Agility;

    [Header("Механические характеристики")]
    public int Level;
    public int MaxHealth;
    public int MaxMana;
    public float MaxStamina;
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
    public float AttackDamage;
    public float AttackRange;
    public float AttackDuration;
    public int Armor;
}
