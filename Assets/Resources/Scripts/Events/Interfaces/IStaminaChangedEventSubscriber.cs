using EventBusSystem;

public interface IStaminaChangedEventSubscriber : IGlobalSubscriber
{
    void OnStaminaChanged(StaminaChangedEvent evt);
}