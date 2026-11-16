public struct ManaChangedEvent
{
    public float CurrentMana { get; }
    public float MaxMana { get; }
    public ManaChangedEvent(float current, float max)
    {
        CurrentMana = current;
        MaxMana = max;
    }
}