public interface IWindowService
{
    void ShowWindow<T>() where T : IViewModel;
    void CloseWindow<T>() where T : IViewModel;
    bool IsWindowOpen<T>() where T : IViewModel;
    void ShowAIDialogue(string npcId);
    void ShowClassicalDialogue(string dialogueId);
}
