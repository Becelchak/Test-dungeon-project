public enum NotificationType
{
    Info,
    XP,
    LevelUp,
    Warning,
    Lore,
    System
}

public struct NotificationAddedEvent
{
    public NotificationData Data { get; }

    public NotificationAddedEvent(NotificationData data)
    {
        Data = data;
    }
}
