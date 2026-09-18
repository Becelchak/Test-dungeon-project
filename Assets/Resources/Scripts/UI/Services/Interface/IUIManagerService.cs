
public interface IUIManagerService
{
    //void OpenInventory();
    //void CloseTop();
    void ToggleInventory();
    void OpenWindow(UILayer layer);
    void CloseTop();
    bool IsAnyUIOpen { get; }
}
