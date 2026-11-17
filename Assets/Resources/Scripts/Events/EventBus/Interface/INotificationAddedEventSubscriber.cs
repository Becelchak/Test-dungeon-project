using EventBusSystem;

public interface INotificationAddedEventSubscriber : IGlobalSubscriber
{
    void OnNotificationAdded(NotificationAddedEvent evt);
}
