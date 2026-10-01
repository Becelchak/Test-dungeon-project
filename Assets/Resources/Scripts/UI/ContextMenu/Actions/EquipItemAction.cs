using EventBusSystem;
using System.Linq;
using UnityEngine;

public class EquipItemAction : IItemAction
{
    public string ActionName => "Экипировать";
    public string ActionDescription => "Экипировать предмет в подходящий слот";

    public bool CanExecute(ItemData item)
    {
        return item != null && (item.itemType == ItemType.Weapon || item.itemType == ItemType.Armor || item.itemType == ItemType.Trinket);
    }

    public void Execute(ItemData item, InventoryItemSlotViewModel itemVm)
    {
        if (item == null) return;

        EquipmentSlotType targetSlot = GetFirstAvailableSlot(item);
        if (targetSlot != EquipmentSlotType.Weapon1)
        {
            EventBus.RaiseEvent<IEquipmentRequestSubscriber>(s =>
                s.OnRequestEquipFromInventory(item, targetSlot));
        }
    }

    private EquipmentSlotType GetFirstAvailableSlot(ItemData item)
    {
        var equipmentService = ServiceLocator.Instance.GetService<IEquipmentService>();
        if (equipmentService == null) return EquipmentSlotType.Weapon1;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                // Ищем первый свободный оружейный слот
                var weaponSlots = new[] { EquipmentSlotType.Weapon1, EquipmentSlotType.Weapon2, EquipmentSlotType.Weapon3 };
                foreach (var slot in weaponSlots)
                {
                    var slotItem = equipmentService.GetItemInSlot(slot);
                    if (slotItem == null) return slot;
                }
                return EquipmentSlotType.Weapon1; // Если все заняты, экипируем в первый

            case ItemType.Armor:
                if (item is ArmorData armor)
                {
                    switch (armor.armorType)
                    {
                        case ArmorType.Head:
                            return EquipmentSlotType.Head;
                        case ArmorType.Body:
                            return EquipmentSlotType.Body;
                        case ArmorType.Legs:
                            return EquipmentSlotType.Legs;
                    }
                }
                return EquipmentSlotType.Body;

            case ItemType.Trinket:
                // Ищем первый свободный слот для тринкетов
                var trinketSlots = new[] { EquipmentSlotType.Trinket1, EquipmentSlotType.Trinket2, EquipmentSlotType.Trinket3 };
                foreach (var slot in trinketSlots)
                {
                    var slotItem = equipmentService.GetItemInSlot(slot);
                    if (slotItem == null) return slot;
                }
                return EquipmentSlotType.Trinket1;

            default:
                return EquipmentSlotType.Weapon1;
        }
    }
}
