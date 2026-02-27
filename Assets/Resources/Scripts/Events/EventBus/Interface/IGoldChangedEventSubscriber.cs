using EventBusSystem;

public interface IGoldChangedEventSubscriber : IGlobalSubscriber
{
    void OnGoldChanged(GoldChangedEvent evt);
}