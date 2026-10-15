using EventBusSystem;

public interface IInteractionTargetLostSubscriber : IGlobalSubscriber
{
    void OnTargetLost();
}
