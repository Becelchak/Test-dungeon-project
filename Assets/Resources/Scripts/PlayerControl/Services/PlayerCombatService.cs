using EventBusSystem;
using System;
using UnityEngine;

/// <summary>
/// Реализация боевого сервиса игрока. Управляет блоком, идеальным блоком и получением урона.
/// </summary>
public class PlayerCombatService : BaseService, IPlayerCombatService
{
    public bool IsBlocking { get; private set; }
    public float BlockStartTime { get; private set; }

    public bool IsParrying { get; private set; }
    public float ParryStartTime { get; private set; }
    public float ParryWindow { get; private set; }

    private IPlayerProfileService _profileService;
    private IEquipmentService _equipmentService;

    private IPlayerProfileService ProfileService => _profileService ??= ServiceLocator.Instance.GetService<IPlayerProfileService>();
    private IEquipmentService EquipmentService => _equipmentService ??= ServiceLocator.Instance.GetService<IEquipmentService>();

    protected override Type GetServiceType() => typeof(IPlayerCombatService);

    public void SetBlocking(bool isBlocking)
    {
        IsBlocking = isBlocking;
        if (isBlocking)
            BlockStartTime = Time.time;
    }

    /// <summary>
    /// Пытается начать парирование. Снимает стамину и открывает короткое окно,
    /// в течение которого полученный урон будет парирован.
    /// </summary>
    /// <returns>true, если парирование удалось начать.</returns>
    public bool TryStartParry()
    {
        var blocker = GetActiveBlocker() as WeaponData;
        float cost = blocker?.Stats?.parryStaminaCost ?? 0f;
        float window = blocker?.Stats?.parryWindow ?? 0.2f;

        if (ProfileService?.CurrentProfile == null)
            return false;

        if (ProfileService.CurrentProfile.stamina < cost)
        {
            Debug.Log("[PlayerCombatService] Недостаточно стамины для парирования.");
            return false;
        }

        ProfileService.ModifyStamina(-Mathf.RoundToInt(cost));
        IsParrying = true;
        ParryStartTime = Time.time;
        ParryWindow = window;
        Debug.Log($"[PlayerCombatService] Парирование ({blocker?.displayName ?? "без оружия"})! Потрачено {cost} стамины, окно {window} сек.");
        return true;
    }

    public void ApplyDamage(int damage, GameObject source = null)
    {
        if (damage <= 0) return;

        int finalDamage = damage;

        // Сначала проверяем парирование — оно приоритетнее и имеет меньшее окно
        if (IsParrying && Time.time - ParryStartTime <= ParryWindow)
        {
            IsParrying = false;
            finalDamage = 0;
            Debug.Log("[PlayerCombatService] Парирование успешно! Урон нивелирован.");
            return;
        }

        if (IsBlocking)
        {
            var blocker = GetActiveBlocker() as WeaponData;
            float window = blocker?.Stats?.perfectBlockWindow ?? 0.5f;
            float reduction = blocker?.Stats?.blockDamageReduction ?? 0.5f;

            if (Time.time - BlockStartTime <= window)
            {
                finalDamage = 0;
                Debug.Log($"[PlayerCombatService] Идеальный блок ({blocker?.displayName ?? "без оружия"})! Урон полностью нивелирован.");
                EventBus.RaiseEvent<IPerfectBlockEventSubscriber>(
                    s => s.OnPerfectBlock(new PerfectBlockEvent(source, blocker))
                );
            }
            else
            {
                finalDamage = Mathf.RoundToInt(damage * (1f - Mathf.Clamp01(reduction)));
                Debug.Log($"[PlayerCombatService] Обычный блок ({blocker?.displayName ?? "без оружия"}). Урон снижен с {damage} до {finalDamage}.");
            }
        }

        if (finalDamage > 0)
        {
            ProfileService?.ModifyHealth(-finalDamage);
            Debug.Log($"[PlayerCombatService] Игрок получил урон: {finalDamage}");
        }
    }

    /// <summary>
    /// Возвращает предмет, которым сейчас выполняется блок: щит (если экипирован и совместим с оружием),
    /// иначе активное оружие.
    /// </summary>
    private ItemData GetActiveBlocker()
    {
        var shield = EquipmentService?.CurrentShield;
        var weapon = EquipmentService?.CurrentWeapon;

        // Щит активен только с одноручным оружием
        if (shield != null && weapon != null && weapon.handling != WeaponHandling.BothHands)
            return shield;

        return weapon;
    }
}
