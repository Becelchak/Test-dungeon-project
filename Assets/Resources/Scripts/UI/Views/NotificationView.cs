using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;

/// <summary>
/// Панель уведомлений (тостов). Показывает CurrentNotification из ViewModel
/// по очереди: отображение -> выдержка -> затухание -> следующее.
/// </summary>
public class NotificationView : BaseView<NotificationViewModel>
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;
    private Coroutine _showRoutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;

        if (ViewModel == null)
        {
            var vm = new NotificationViewModel();
            vm.Initialize();
            Bind(vm);
        }
    }

    protected override void SetupBindings()
    {
        ApplyNotification(ViewModel.CurrentNotification);
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentNotification))
            ApplyNotification(ViewModel.CurrentNotification);
    }

    private void ApplyNotification(NotificationData data)
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        if (data == null)
        {
            _canvasGroup.alpha = 0f;
            return;
        }

        if (notificationText == null)
        {
            Debug.LogWarning("[NotificationView] Не назначен TextMeshProUGUI!");
            ViewModel?.CompleteCurrent();
            return;
        }

        notificationText.text = string.IsNullOrEmpty(data.Title)
            ? data.Message
            : $"{data.Title}\n{data.Message}";
        notificationText.color = GetTypeColor(data.Type);

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        _canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;

        ViewModel?.CompleteCurrent();
    }

    private static Color GetTypeColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.XP => new Color(0.4f, 0.9f, 0.4f),
            NotificationType.LevelUp => new Color(1f, 0.85f, 0.3f),
            NotificationType.Warning => new Color(1f, 0.4f, 0.4f),
            NotificationType.Lore => new Color(0.7f, 0.7f, 1f),
            _ => Color.darkRed,
        };
    }
}
