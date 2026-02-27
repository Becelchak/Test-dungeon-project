using EventBusSystem;
using TMPro;
using UnityEngine;

public class GoldDisplay : MonoBehaviour, IGoldChangedEventSubscriber
{
    [SerializeField] private TextMeshProUGUI goldText;

    public void OnGoldChanged(GoldChangedEvent evt)
    {
        UpdateGold(evt.NewGold);
    }

    private void OnEnable()
    {
        EventBus.Subscribe(this);

        var profile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        UpdateGold(profile.CurrentProfile.goldCount);
    }

    private void OnDisable() => EventBus.Unsubscribe(this);

    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"Золото: {gold}";
    }
}