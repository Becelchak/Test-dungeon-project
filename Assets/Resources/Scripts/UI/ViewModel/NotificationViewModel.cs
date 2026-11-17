using EventBusSystem;
using System.Collections.Generic;

/// <summary>
/// ViewModel панели уведомлений. Держит очередь входящих уведомлений
/// и предоставляет текущее для отображения.
/// </summary>
public class NotificationViewModel : BaseViewModel, INotificationAddedEventSubscriber
{
    private const int MaxQueueSize = 10;

    private readonly Queue<NotificationData> _queue = new();

    private NotificationData _currentNotification;
    public NotificationData CurrentNotification
    {
        get => _currentNotification;
        private set => SetProperty(ref _currentNotification, value);
    }

    public override void Initialize()
    {
        EventBus.Subscribe(this);
    }

    public override void Cleanup()
    {
        EventBus.Unsubscribe(this);
        _queue.Clear();
        CurrentNotification = null;
    }

    public void OnNotificationAdded(NotificationAddedEvent evt)
    {
        if (evt.Data == null) return;

        if (_queue.Count >= MaxQueueSize)
            _queue.Dequeue();

        _queue.Enqueue(evt.Data);

        if (CurrentNotification == null)
            ShowNext();
    }

    /// <summary>
    /// Вызывается View по завершении показа текущего уведомления.
    /// </summary>
    public void CompleteCurrent()
    {
        if (CurrentNotification == null) return;
        ShowNext();
    }

    private void ShowNext()
    {
        CurrentNotification = _queue.Count > 0 ? _queue.Dequeue() : null;
    }
}
