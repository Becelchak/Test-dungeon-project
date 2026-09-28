
using UnityEngine;

/// <summary>Глобальная «булавка» — что сейчас тащат.</summary>
public static class ItemDragPayload
{
    public static ItemDragPayloadData Current { get; set; }

    public static DragSourceType Source { get; set; }
    public static InventoryItemSlotViewModel InventoryItemVm { get; set; }
    public static EquipmentSlotType EquipmentSlotType { get; set; }
    public static Sprite ItemIcon { get; set; }
}

public enum DragSourceType { Inventory, Equipment }

public class ItemDragPayloadData
{
    public InventoryItemSlotViewModel ItemVm;
    public InventoryViewModel Source;

    public ItemDragPayloadData(InventoryItemSlotViewModel itemVm, InventoryViewModel source)
    {
        ItemVm = itemVm;
        Source = source;
    }
}