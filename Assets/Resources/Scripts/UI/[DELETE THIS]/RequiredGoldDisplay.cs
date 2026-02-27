using EventBusSystem;
using TMPro;
using UnityEngine;

public class RequiredGoldDisplay : MonoBehaviour, IGoldChangedEventSubscriber
{
    [SerializeField] private TextMeshProUGUI requiredText;
    private int _requiredGold;

    private void Start()
    {
        var dungeonManager = ServiceLocator.Instance.GetService<IDungeonManagerService>();
        _requiredGold = dungeonManager.RequiredGold;
        UpdateText(0);
    }

    private void OnEnable() => EventBus.Subscribe(this);
    private void OnDisable() => EventBus.Unsubscribe(this);

    public void OnGoldChanged(GoldChangedEvent evt) => UpdateText(evt.NewGold);

    private void UpdateText(int currentGold) => requiredText.text = $"Цель: {currentGold}/{_requiredGold} G";
}