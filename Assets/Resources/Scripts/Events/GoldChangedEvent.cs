public struct GoldChangedEvent
{
    public int NewGold { get; }
    public GoldChangedEvent(int newGold) => NewGold = newGold;
}