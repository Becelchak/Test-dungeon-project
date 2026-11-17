public interface INotificationService
{
    void Notify(NotificationType type, string title, string message);
}
