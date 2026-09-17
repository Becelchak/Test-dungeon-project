public struct ManaChangedEvent
{
    public int CurrentMana { get; }
    public int MaxMana { get; }
    public ManaChangedEvent(int current, int max)
    {
        CurrentMana = current;
        MaxMana = max;
    }
}