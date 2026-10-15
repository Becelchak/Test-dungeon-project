using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Приёмник урона игрока. Реализует IDamageable и перенаправляет урон
/// в PlayerCombatService, где учитываются блок, парирование и стамина.
/// Вешается на корневой объект игрока.
/// </summary>
public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    private ITriggerService _triggerService;
    public Transform Transform => transform;

    public void Start()
    {
        _triggerService = ServiceLocator.Instance.GetService<ITriggerService>();
    }

    public void ApplyDamage(DamageInfo damageInfo)
    {
        if (damageInfo.FinalDamage <= 0) return;

        var combatService = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        if (combatService != null)
        {
            var evt = new DamageTakenEvent { Amount = damageInfo.BaseDamage, Attacker = damageInfo.Attacker.gameObject, Type = damageInfo.DamageType };
            _triggerService.Dispatch(evt, gameObject);

            // Если есть триггер отмены урона и он сработал - отменить урон
            if (evt.Amount <= 0) return;
            combatService.ApplyDamage(damageInfo);
        }
        else
        {
            // Fallback: если боевой сервис по какой-то причине недоступен
            int finalDamage = Mathf.RoundToInt(damageInfo.FinalDamage);
            var profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
            profileService?.ModifyHealth(-finalDamage);
            Debug.LogWarning($"[PlayerDamageReceiver] Боевой сервис не найден, урон {finalDamage} применён напрямую к здоровью.");
        }
    }
}
