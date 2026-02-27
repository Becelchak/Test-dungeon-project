public struct InteractionPromptEvent
{
    public bool Show { get; }
    public string PromptText { get; }

    public InteractionPromptEvent(IInteractable inter)
    {
        Show = true;
        PromptText = inter?.GetInteractionPrompt() ?? "";
    }

    public InteractionPromptEvent(bool isContinue)
    {
        Show = false;
        PromptText = null;
    }
}