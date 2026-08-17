using UnityEngine;

/// <summary>
/// Приёмник урона игрока. Реализует IDamageable и перенаправляет урон
/// в PlayerCombatService, где учитываются блок, парирование и стамина.
/// Вешается на корневой объект игрока.
/// </summary>
public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    public Transform Transform => transform;

    public void ApplyDamage(DamageInfo damageInfo)
    {
        int finalDamage = Mathf.RoundToInt(damageInfo.FinalDamage);
        if (finalDamage <= 0) return;

        var combatService = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        if (combatService != null)
        {
            combatService.ApplyDamage(finalDamage, damageInfo.Attacker != null ? damageInfo.Attacker.gameObject : null);
        }
        else
        {
            // Fallback: если боевой сервис по какой-то причине недоступен
            var profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
            profileService?.ModifyHealth(-finalDamage);
            Debug.LogWarning($"[PlayerDamageReceiver] Боевой сервис не найден, урон {finalDamage} применён напрямую к здоровью.");
        }
    }
}
