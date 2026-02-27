using EventBusSystem;

public struct LevelDefeatEvent { }
public interface ILevelDefeatEventSubscriber : IGlobalSubscriber { void OnLevelDefeat(); }