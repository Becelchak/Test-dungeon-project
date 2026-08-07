using System;
using UnityEngine;

[Serializable]
public class EquipmentSlot
{
    [Tooltip("Тип экипировочного слота")]
    public EquipmentSlotType slotType;
    [Tooltip("Тип предмета, который можно поместить в слот")]
    public ItemType allowedItemType;
    [Tooltip("Разблокирован ли слот")]
    public bool isUnlocked = true;
    [SerializeField]
    private ItemData _item;

    public ItemData Item => _item;
    public bool IsOccupied => _item != null;

    public EquipmentSlot(EquipmentSlotType type, ItemType allowedType, bool unlocked = true)
    {
        slotType = type;
        allowedItemType = allowedType;
        isUnlocked = unlocked;
        _item = null;
    }

    public bool CanAccept(ItemData item)
    {
        if (!isUnlocked || item == null)
            return false;

        return item.itemType == allowedItemType;
    }

    public bool Equip(ItemData item)
    {
        if (!CanAccept(item))
            return false;

        _item = item;
        return true;
    }

    public ItemData Unequip()
    {
        var previousItem = _item;
        _item = null;
        return previousItem;
    }
}
