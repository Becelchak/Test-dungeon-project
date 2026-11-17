using EventBusSystem;

public interface IEnemyKilledEventSubscriber : IGlobalSubscriber
{
    void OnEnemyKilled(EnemyKilledEvent evt);
}
