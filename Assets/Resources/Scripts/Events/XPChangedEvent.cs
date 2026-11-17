public struct XPChangedEvent
{
    public int AmountGained { get; }
    public int CurrentXP { get; }
    public int NewLevel { get; }
    public XPSource Source { get; }

    public XPChangedEvent(int amountGained, int currentXP, int newLevel, XPSource source)
    {
        AmountGained = amountGained;
        CurrentXP = currentXP;
        NewLevel = newLevel;
        Source = source;
    }
}
