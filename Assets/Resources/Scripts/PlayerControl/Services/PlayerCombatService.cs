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

    public void ApplyDamage(int damage, GameObject source = null)
    {
        if (damage <= 0) return;

        int finalDamage = damage;

        if (IsBlocking)
        {
            var weapon = EquipmentService?.CurrentWeapon;
            float window = weapon?.Stats?.perfectBlockWindow ?? 0.5f;
            float reduction = weapon?.Stats?.blockDamageReduction ?? 0.5f;

            if (Time.time - BlockStartTime <= window)
            {
                finalDamage = 0;
                Debug.Log("[PlayerCombatService] Идеальный блок! Урон полностью нивелирован.");
                EventBus.RaiseEvent<IPerfectBlockEventSubscriber>(
                    s => s.OnPerfectBlock(new PerfectBlockEvent(source, weapon))
                );
            }
            else
            {
                finalDamage = Mathf.RoundToInt(damage * (1f - Mathf.Clamp01(reduction)));
                Debug.Log($"[PlayerCombatService] Обычный блок. Урон снижен с {damage} до {finalDamage}.");
            }
        }

        if (finalDamage > 0)
        {
            ProfileService?.ModifyHealth(-finalDamage);
            Debug.Log($"[PlayerCombatService] Игрок получил урон: {finalDamage}");
        }
    }
}
