public struct RespawnIntervalChangedEvent
{
    public string Message { get; }
    public float Duration { get; }

    public RespawnIntervalChangedEvent(string message, float duration = 0f)
    {
        Message = message;
        Duration = duration;
    }
}