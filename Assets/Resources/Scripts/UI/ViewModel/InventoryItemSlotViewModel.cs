using UnityEngine;

public class InventoryItemSlotViewModel
{
    public InventoryItem RuntimeItem { get; }
    public ItemData Data { get; }

    public InventoryItemSlotViewModel(InventoryItem runtimeItem, ItemData data)
    {
        RuntimeItem = runtimeItem;
        Data = data;
    }

    public string ItemId => RuntimeItem.itemId;
    public string DisplayName => Data != null ? Data.displayName : RuntimeItem.itemName;
    public string Description => Data != null ? Data.description : RuntimeItem.description;
    public Sprite Icon => Data?.icon;
    public int Quantity => RuntimeItem.quantity;
    public bool IsStackable => Quantity > 1;
}
