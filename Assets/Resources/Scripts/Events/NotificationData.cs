public class NotificationData
{
    public NotificationType Type { get; }
    public string Title { get; }
    public string Message { get; }

    public NotificationData(NotificationType type, string title, string message)
    {
        Type = type;
        Title = title;
        Message = message;
    }
}
