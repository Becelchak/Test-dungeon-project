using EventBusSystem;

public interface IXPChangedEventSubscriber : IGlobalSubscriber
{
    void OnXPChanged(XPChangedEvent evt);
}
