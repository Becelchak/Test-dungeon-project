using EventBusSystem;

public struct EmotionsUpdatedEvent
{
    public float[] Emotions { get; }
    public EmotionsUpdatedEvent(float[] emotions) => Emotions = emotions;
}

public interface IEmotionsUpdatedSubscriber : IGlobalSubscriber
{
    void OnEmotionsUpdated(EmotionsUpdatedEvent evt);
}