using EventBusSystem;

public interface ILevelUpEventSubscriber : IGlobalSubscriber
{
    void OnLevelUp(LevelUpEvent evt);
}
