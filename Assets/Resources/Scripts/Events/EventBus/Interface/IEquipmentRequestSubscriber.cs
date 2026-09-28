using EventBusSystem;

public interface IEquipmentRequestSubscriber : IGlobalSubscriber
{
    void OnRequestEquipFromInventory(ItemData item, EquipmentSlotType targetSlot);
    void OnRequestUnequipToInventory(InventoryItem item, EquipmentSlotType slotType);
}