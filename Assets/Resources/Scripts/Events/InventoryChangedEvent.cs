using EventBusSystem;
using UnityEngine;

public struct InventoryChangedEvent : IInventoryChangedEventSubscriber
{
    public enum ChangeType { Added, Removed, Updated }
    public ChangeType Type { get; }
    public InventoryItem Item { get; }

    public InventoryChangedEvent(ChangeType type, InventoryItem item)
    {
        Type = type;
        Item = item;
    }

    public void OnInventoryChanged(InventoryChangedEvent evt)
    {
        Debug.LogWarning("Инвентарь поменялся");
    }
}

public interface IInventoryChangedEventSubscriber : IGlobalSubscriber
{
    void OnInventoryChanged(InventoryChangedEvent evt);
}