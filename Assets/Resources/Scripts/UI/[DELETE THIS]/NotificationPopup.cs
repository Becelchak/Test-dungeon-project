using EventBusSystem;
using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationPopup : MonoBehaviour, IRespawnIntervalChangedEvent
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 3f;

    private Coroutine _hideCoroutine;

    private void OnEnable()
    {
        EventBus.Subscribe(this);
        notificationPanel.SetActive(false);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
    }

    private void OnRespawnIntervalChanged(RespawnIntervalChangedEvent evt)
    {
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        notificationText.text = evt.Message;
        notificationPanel.SetActive(true);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationPanel.SetActive(false);
    }
}