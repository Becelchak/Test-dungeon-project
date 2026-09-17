using EventBusSystem;

public interface IManaChangedEventSubscriber : IGlobalSubscriber
{
    void OnManaChanged(ManaChangedEvent evt);
}