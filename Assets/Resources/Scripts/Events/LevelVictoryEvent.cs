using EventBusSystem;

public struct LevelVictoryEvent { }
public interface ILevelVictoryEventSubscriber : IGlobalSubscriber { void OnLevelVictory(); }