public struct RespawnIntervalChangedEvent
{
    public float NewInterval { get; }
    public string Message { get; }

    public RespawnIntervalChangedEvent(float newInterval, string message)
    {
        NewInterval = newInterval;
        Message = message;
    }
}