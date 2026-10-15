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

        if (targetSlot == EquipmentSlotType.Unknown)
        {
            Debug.Log($"[EquipItemAction] Аналогичный предмет уже экипирован! Повторно надеть такой же нельзя!");
            return;
        }
        if (targetSlot == EquipmentSlotType.Weapon1 && item.itemType != ItemType.Weapon)
        {
            // Если вернулся дефолтный слот, но это не оружие - значит нет подходящего слота
            Debug.LogWarning($"[EquipItemAction] Нет подходящего слота для {item.displayName}");
            return;
        }

        Debug.Log($"[EquipItemAction] Попытка экипировать {item.displayName} в слот {targetSlot}");

        EventBus.RaiseEvent<IEquipmentRequestSubscriber>(s =>
            s.OnRequestEquipFromInventory(item, targetSlot));
    }

    private EquipmentSlotType GetFirstAvailableSlot(ItemData item)
    {
        var equipmentService = ServiceLocator.Instance.GetService<IEquipmentService>();
        if (equipmentService == null) return EquipmentSlotType.Weapon1;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                var weaponSlots = new[] { EquipmentSlotType.Weapon1, EquipmentSlotType.Weapon2, EquipmentSlotType.Weapon3 };
                foreach (var slot in weaponSlots)
                {
                    var slotItem = equipmentService.GetItemInSlot(slot);
                    if (slotItem != null && slotItem.itemId == item.itemId)
                    {
                        Debug.LogWarning($"[EquipItemAction] Предмет {item.displayName} уже экипирован в слоте {slot}");
                        return EquipmentSlotType.Unknown; // Возврат пустого слота, поскольку экипировать в новый то же оружие нельзя
                    }
                }

                foreach (var slot in weaponSlots)
                {
                    var slotItem = equipmentService.GetItemInSlot(slot);
                    if (slotItem == null) return slot;
                }
                // Если все заняты - возврат пустого слота
                return EquipmentSlotType.Unknown;

            case ItemType.Armor:
                if (item is ArmorData armor)
                {
                    var targetArmorSlot = armor.armorType switch
                    {
                        ArmorType.Head => EquipmentSlotType.Head,
                        ArmorType.Body => EquipmentSlotType.Body,
                        ArmorType.Legs => EquipmentSlotType.Legs,
                        _ => EquipmentSlotType.Body
                    };

                    var existingArmor = equipmentService.GetItemInSlot(targetArmorSlot);
                    if (existingArmor != null && existingArmor.itemId == item.itemId)
                    {
                        Debug.LogWarning($"[EquipItemAction] Предмет {item.displayName} уже экипирован в слоте {targetArmorSlot}");
                        return EquipmentSlotType.Weapon1;
                    }

                    return targetArmorSlot;
                }
                return EquipmentSlotType.Body;

            case ItemType.Trinket:
                var trinketSlots = new[] { EquipmentSlotType.Trinket1, EquipmentSlotType.Trinket2, EquipmentSlotType.Trinket3 };
                foreach (var slot in trinketSlots)
                {
                    var slotItem = equipmentService.GetItemInSlot(slot);
                    if (slotItem != null && slotItem.itemId == item.itemId)
                    {
                        Debug.LogWarning($"[EquipItemAction] Предмет {item.displayName} уже экипирован в слоте {slot}");
                        return EquipmentSlotType.Unknown; // Возврат пустого слота
                    }
                }

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
