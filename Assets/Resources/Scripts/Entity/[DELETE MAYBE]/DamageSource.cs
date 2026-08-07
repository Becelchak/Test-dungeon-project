using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private float cooldown = 1f;

    private bool _canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!CanDamage(other)) return;
        ApplyDamage();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanDamage(collision.collider)) return;
        ApplyDamage();
    }

    private bool CanDamage(Collider collider)
    {
        return _canDamage && collider.CompareTag("Player");
    }

    private void ApplyDamage()
    {
        var combatService = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        if (combatService != null)
        {
            combatService.ApplyDamage(damageAmount, gameObject);
        }
        else
        {
            // Fallback: если сервиса нет, наносим урон напрямую
            var playerService = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
            playerService.ModifyHealth(-damageAmount);
            Debug.Log("ИГРОК ПОЛУЧИЛ УРОН! [fallback]");
        }

        _canDamage = false;
        Invoke(nameof(ResetCooldown), cooldown);

        if (destroyOnHit) Destroy(gameObject);
    }

    private void ResetCooldown() => _canDamage = true;
}
