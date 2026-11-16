using EventBusSystem;

public struct HealthChangedEvent
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public HealthChangedEvent(float current, float max) { CurrentHealth = current; MaxHealth = max; }
}

public interface IHealthChangedEventSubscriber : IGlobalSubscriber
{
    void OnHealthChanged(HealthChangedEvent evt);
}