using EventBusSystem;
using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour, IHealthChangedEventSubscriber
{
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        var profile = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
        UpdateHealth(profile.CurrentProfile.health, profile.CurrentProfile.maxHealth);
    }

    private void OnDisable() => EventBus.Unsubscribe(this);

    public void OnHealthChanged(HealthChangedEvent evt) => UpdateHealth(evt.CurrentHealth, evt.MaxHealth);

    private void UpdateHealth(int current, int max) => healthText.text = $"Çהמנמגüו: {current}/{max}";
}