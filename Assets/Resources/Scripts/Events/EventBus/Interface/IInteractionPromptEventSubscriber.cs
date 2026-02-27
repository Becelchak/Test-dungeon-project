using EventBusSystem;

public interface IInteractionPromptEventSubscriber : IGlobalSubscriber
{
    void OnInteractionPrompt(InteractionPromptEvent evt);
}
