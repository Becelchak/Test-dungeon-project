using System;
using System.Collections.Generic;
using System.Linq;

public class EquipmentViewModel : BaseViewModel
{
    private IEquipmentService _equipment;
    public IReadOnlyList<EquipmentSlotViewModel> Slots => _slots;
    private readonly List<EquipmentSlotViewModel> _slots = new();
    public int ActiveWeaponSlotIndex => _equipment.ActiveWeaponSlotIndex;

    public event Action OnSlotsRebuilt;
    public event Action OnActiveWeaponChanged;

    public override void Initialize()
    {
        _equipment = ServiceLocator.Instance.GetService<IEquipmentService>();
        if (_equipment == null)
        {
            UnityEngine.Debug.LogError("[EquipmentViewModel] IEquipmentService не найден!");
            return;
        }

        _equipment.OnEquipmentChanged += HandleEquipmentChanged;
        _equipment.OnWeaponChanged += HandleWeaponChanged;

        BuildSlots();
    }

    public override void Cleanup()
    {
        if (_equipment != null)
        {
            _equipment.OnEquipmentChanged -= HandleEquipmentChanged;
            _equipment.OnWeaponChanged -= HandleWeaponChanged;
        }
        foreach (var slot in _slots)
            slot.OnChanged -= HandleSlotChanged;
        _slots.Clear();
    }

    // -----------------------------------------------------------------
    // Публичное API для View
    // -----------------------------------------------------------------

    public EquipmentSlotViewModel GetSlot(EquipmentSlotType type)
        => _slots.FirstOrDefault(s => s.SlotType == type);

    public void SetActiveWeaponSlot(int index)
    {
        _equipment.SetActiveWeaponSlot(index);
    }

    /// <summary>
    /// Перетаскивание предмета в слот (для drop-хендлера на UI).
    /// </summary>
    public bool TryEquipToSlot(EquipmentSlotType targetSlot, ItemData item)
    {
        var slotVm = GetSlot(targetSlot);
        if (slotVm == null) return false;
        return slotVm.TryEquip(item);
    }

    /// <summary>
    /// Снять предмет из слота и, например, вернуть его в инвентарь.
    /// </summary>
    public ItemData TryUnequipFromSlot(EquipmentSlotType slotType)
    {
        var slotVm = GetSlot(slotType);
        return slotVm?.TryUnequip();
    }

    // -----------------------------------------------------------------
    // Внутреннее
    // -----------------------------------------------------------------

    private void BuildSlots()
    {
        foreach (var slot in _slots)
            slot.OnChanged -= HandleSlotChanged;
        _slots.Clear();

        foreach (var slotData in _equipment.Slots)
        {
            var vm = new EquipmentSlotViewModel(slotData, _equipment);
            vm.OnChanged += HandleSlotChanged;
            _slots.Add(vm);
        }

        OnSlotsRebuilt?.Invoke();
    }

    private void HandleEquipmentChanged(EquipmentSlotType slotType, ItemData item)
    {
        var slot = GetSlot(slotType);
        slot?.NotifyChanged();
        OnActiveWeaponChanged?.Invoke();
    }

    private void HandleWeaponChanged(WeaponData weapon)
    {
        OnActiveWeaponChanged?.Invoke();
    }

    private void HandleSlotChanged()
    {
        // Можно уведомить View о том, что конкретный слот изменился
    }
}