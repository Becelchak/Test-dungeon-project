using EventBusSystem;

public struct HealthChangedEvent
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public HealthChangedEvent(int current, int max) { CurrentHealth = current; MaxHealth = max; }
}

public interface IHealthChangedEventSubscriber : IGlobalSubscriber
{
    void OnHealthChanged(HealthChangedEvent evt);
}