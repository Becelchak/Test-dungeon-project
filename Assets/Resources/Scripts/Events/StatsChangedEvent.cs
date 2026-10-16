using EventBusSystem;

public struct StatsChangedEvent
{
    public FinalPlayerStats Stats;

    public StatsChangedEvent(FinalPlayerStats stats)
    {
        Stats = stats;
    }
}

public interface IStatsChangedEventSubscriber : IGlobalSubscriber
{
    void OnStatsChanged(StatsChangedEvent evt);
}
