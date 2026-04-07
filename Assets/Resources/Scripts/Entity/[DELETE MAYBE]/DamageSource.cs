using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private float cooldown = 1f;

    private float _lastDamageTime;
    private bool _canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!_canDamage || !other.CompareTag("Player")) return;

        var playerService = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        playerService.ModifyHealth(-damageAmount);
        Debug.Log("»√–Œ  œŒÀ”◊»À ”–ŒÕ! [“–»√√≈–]");

        _canDamage = false;
        Invoke(nameof(ResetCooldown), cooldown);

        if (destroyOnHit) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_canDamage || !collision.collider.CompareTag("Player")) return;

        var playerService = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
        playerService.ModifyHealth(-damageAmount);
        Debug.Log("»√–Œ  œŒÀ”◊»À ”–ŒÕ! [—ŒÀÀ»ƒ≈–]");

        _canDamage = false;
        Invoke(nameof(ResetCooldown), cooldown);

        if (destroyOnHit) Destroy(gameObject);
    }

    private void ResetCooldown() => _canDamage = true;
}