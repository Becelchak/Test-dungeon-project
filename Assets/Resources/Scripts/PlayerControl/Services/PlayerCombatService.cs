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
    public bool IsDead { get; private set; }
    public bool IsGodMode { get; set; }

    private IPlayerProfileService _profileService;
    private IEquipmentService _equipmentService;
    private WeaponIKController _weaponIKController;
    private Animator _animatorController;
    private PlayerAnimationController _playerAnimationController;

    private IPlayerProfileService ProfileService => _profileService ??= ServiceLocator.Instance.GetService<IPlayerProfileService>();
    private IEquipmentService EquipmentService => _equipmentService ??= ServiceLocator.Instance.GetService<IEquipmentService>();
    private WeaponIKController WeaponIK => _weaponIKController ??= UnityEngine.Object.FindObjectOfType<WeaponIKController>();
    private Animator PlayerAnimator => WeaponIK?.PlayerAnimator;
    private PlayerAnimationController PlayerAnimController => _playerAnimationController ??= WeaponIK?.GetComponent<PlayerAnimationController>();

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
        if (damage <= 0 || IsDead) return;
        if (IsGodMode)
        {
            Debug.Log("[PlayerCombatService] GodMode: урон игнорирован.");
            return;
        }

        int finalDamage = damage;

        // Сначала проверяем парирование — оно приоритетнее и имеет меньшее окно
        if (IsParrying && Time.time - ParryStartTime <= ParryWindow)
        {
            IsParrying = false;
            Debug.Log("[PlayerCombatService] Парирование успешно! Урон нивелирован.");
            var playerRoot = WeaponIK != null ? WeaponIK.gameObject : gameObject;
            EventBus.RaiseEvent<IParryEventSubscriber>(x => x.OnParryEvent(new ParrySuccessEvent(playerRoot, source)));
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
            PlayerAnimController?.PlayHitAnimation();
            Debug.Log($"[PlayerCombatService] Игрок получил урон: {finalDamage}. Текущее здоровье {ProfileService.CurrentProfile.health}");

            if (ProfileService.CurrentProfile.health <= 0)
                Die(source);
        }
    }

    private void Die(GameObject source = null)
    {
        if (IsDead) return;
        IsDead = true;

        PlayerAnimController?.TriggerDeath();
        EventBus.RaiseEvent<IPlayerDiedEventSubscriber>(
            s => s.OnPlayerDied(new PlayerDiedEvent(Vector3.zero))
        );
        Debug.Log("[PlayerCombatService] Игрок погиб.");
    }

    public bool TryStartAttack(out bool isWeakAttack)
    {
        isWeakAttack = false;
        if (IsDead) return false;

        var weapon = EquipmentService.CurrentWeapon;
        float cost = weapon?.Stats?.attackStaminaCost ?? 0f;
        int stamina = ProfileService?.CurrentProfile?.stamina ?? 0;

        if (stamina >= cost)
        {
            ProfileService?.ModifyStamina(-Mathf.RoundToInt(cost));
            return true;
        }

        // Не хватает стамины: слабая атака, снимаем остатки, если они есть
        if (stamina > 0)
            ProfileService?.ModifyStamina(-stamina);

        isWeakAttack = true;
        return true;
    }

    public void Revive()
    {
        IsDead = false;
        if (ProfileService?.CurrentProfile != null)
        {
            ProfileService.ModifyHealth(ProfileService.CurrentProfile.maxHealth);
            ProfileService.ModifyStamina(ProfileService.CurrentProfile.maxStamina);
        }
        Debug.Log("[PlayerCombatService] Игрок воскрешён.");
    }

    public void SetWeaponDamageSource(bool isAttack, bool isWeakAttack = false)
    {
        var source = WeaponIK?.CurrentWeaponDamageSource;
        if (source == null)
        {
            Debug.LogWarning("[PlayerCombatService] Не найден WeaponDamageSource на текущем runtime-оружии. Убедись, что на префабе оружия есть компонент WeaponDamageSource с триггер-коллайдером.");
            return;
        }

        var weapon = EquipmentService.CurrentWeapon;
        if (weapon != null && weapon.Stats != null)
        {
            float damage = weapon.Stats.damage;
            if (isWeakAttack)
                damage *= weapon.Stats.weakAttackDamageMultiplier;
            source.BaseDamage = damage;
        }

        if (isAttack)
            source.ResetHits();

        source.IsActive = isAttack;
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
