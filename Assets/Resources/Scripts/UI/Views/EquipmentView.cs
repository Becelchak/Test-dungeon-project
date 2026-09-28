using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EquipmentView : BaseView<EquipmentViewModel>
{
    [Header("Static Scene Slots Configuration")]
    [SerializeField] private List<EquipmentSlotUI> staticSceneSlots = new List<EquipmentSlotUI>();

    private readonly Dictionary<EquipmentSlotType, EquipmentSlotUI> _slotUIs = new();

    protected override void SetupBindings()
    {
        ViewModel.OnSlotsRebuilt += BindStaticSlots;
        ViewModel.OnActiveWeaponChanged += RefreshActiveHighlight;
        BindStaticSlots();
    }

    private void OnDestroy()
    {
        if (ViewModel != null)
        {
            ViewModel.OnSlotsRebuilt -= BindStaticSlots;
            ViewModel.OnActiveWeaponChanged -= RefreshActiveHighlight;
        }
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e) { }

    /// <summary>
    /// Связка рантайм-ViewModel с объектами на сцене
    /// </summary>
    private void BindStaticSlots()
    {
        _slotUIs.Clear();

        foreach (var slotUi in staticSceneSlots)
        {
            if (slotUi == null) continue;

            var slotVm = ViewModel.GetSlot(slotUi.SlotType);
            if (slotVm != null)
            {
                slotUi.Bind(slotVm, ViewModel);
                _slotUIs[slotUi.SlotType] = slotUi;
            }
            else
            {
                Debug.LogWarning($"[EquipmentView] Не найдена ViewModel для слота типа: {slotUi.SlotType}!");
            }
        }

        RefreshActiveHighlight();
    }

    private void RefreshActiveHighlight()
    {
        int active = ViewModel.ActiveWeaponSlotIndex;
        foreach (var kv in _slotUIs)
        {
            bool isWeaponSlot =
                kv.Key == EquipmentSlotType.Weapon1 ||
                kv.Key == EquipmentSlotType.Weapon2 ||
                kv.Key == EquipmentSlotType.Weapon3;

            bool activeHighlight = isWeaponSlot &&
                (int)(kv.Key - EquipmentSlotType.Weapon1) == active;

            kv.Value.SetActiveHighlight(activeHighlight);
        }
    }
}
