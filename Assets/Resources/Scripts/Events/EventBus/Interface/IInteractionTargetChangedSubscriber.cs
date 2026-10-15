using EventBusSystem;

public interface IInteractionTargetChangedSubscriber : IGlobalSubscriber
{
    void OnTargetChanged(IInteractable target);
}
