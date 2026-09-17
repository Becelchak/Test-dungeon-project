using EventBusSystem;

public struct InventoryChangedEvent
{
    public enum ChangeType { Added, Removed, Updated }
    public ChangeType Type { get; }
    public InventoryItem Item { get; }

    public InventoryChangedEvent(ChangeType type, InventoryItem item)
    {
        Type = type;
        Item = item;
    }
}

public interface IInventoryChangedEventSubscriber : IGlobalSubscriber
{
    void OnInventoryChanged(InventoryChangedEvent evt);
}