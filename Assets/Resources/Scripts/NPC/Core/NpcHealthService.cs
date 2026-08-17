using System;
using UnityEngine;

/// <summary>
/// Сервис здоровья NPC. Отслеживает текущее/максимальное HP и рассылает события.
/// </summary>
public class NpcHealthService : MonoBehaviour
{
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDamaged;
    public event Action OnDeath;

    public void Initialize(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void ApplyDamage(DamageInfo damageInfo)
    {
        int damage = Mathf.RoundToInt(damageInfo.FinalDamage);
        if (damage <= 0) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnDamaged?.Invoke();

        if (CurrentHealth <= 0)
            OnDeath?.Invoke();
    }
}
