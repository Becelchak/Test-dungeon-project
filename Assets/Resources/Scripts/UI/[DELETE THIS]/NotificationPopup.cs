using EventBusSystem;
using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationPopup : MonoBehaviour, IRespawnIntervalChangedEventSubscriber
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 3f;

    private Coroutine _hideCoroutine;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
    }

    public void OnShowNotification(RespawnIntervalChangedEvent evt)
    {
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

        notificationText.text = evt.Message;
        notificationPanel.SetActive(true);

        float duration = evt.Duration > 0 ? evt.Duration : displayDuration;
        _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notificationPanel.SetActive(false);
    }
}