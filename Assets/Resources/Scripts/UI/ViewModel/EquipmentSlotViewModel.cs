using System;

public class EquipmentSlotViewModel
{
    public EquipmentSlotType SlotType { get; }
    public bool IsUnlocked => _slot.isUnlocked;

    private readonly EquipmentSlot _slot;
    private readonly IEquipmentService _equipment;

    public event Action OnChanged;

    public EquipmentSlotViewModel(EquipmentSlot slot, IEquipmentService equipment)
    {
        _slot = slot ?? throw new ArgumentNullException(nameof(slot));
        _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        SlotType = slot.slotType;
    }

    public ItemData Item => _slot.Item;
    public bool IsOccupied => _slot.IsOccupied;
    public string DisplayName => _slot.Item != null ? _slot.Item.displayName : string.Empty;
    public UnityEngine.Sprite Icon => _slot.Item != null ? _slot.Item.icon : null;

    /// <summary>
    /// Попытка экипировать предмет в этот слот. Возвращает false, если тип не подходит.
    /// </summary>
    public bool TryEquip(ItemData item)
    {
        bool result = _equipment.Equip(item, SlotType);
        if (result) NotifyChanged();
        return result;
    }

    /// <summary>
    /// Снять предмет из слота. Возвращает снятый предмет (или null).
    /// </summary>
    public ItemData TryUnequip()
    {
        if (!_slot.IsOccupied) return null;

        var removed = _slot.Item;
        bool result = _equipment.Unequip(SlotType);
        if (result) NotifyChanged();
        return result ? removed : null;
    }

    public void NotifyChanged() => OnChanged?.Invoke();
}