using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EquipmentView : BaseView<EquipmentViewModel>
{
    [Header("Static Scene Slots Configuration")]
    [SerializeField] private List<EquipmentSlotUI> staticSceneSlots = new List<EquipmentSlotUI>();

    [Header("Info Panel")]
    [Tooltip("GameObject с компонентом AdaptiveInfoPanel")]
    [SerializeField] private GameObject infoPanel;

    private readonly Dictionary<EquipmentSlotType, EquipmentSlotUI> _slotUIs = new();
    private AdaptiveInfoPanel _infoPanel;

    protected override void SetupBindings()
    {
        ViewModel.OnSlotsRebuilt += BindStaticSlots;
        ViewModel.OnActiveWeaponChanged += RefreshActiveHighlight;
        if (infoPanel != null)
        {
            _infoPanel = infoPanel.GetComponent<AdaptiveInfoPanel>();
            if (_infoPanel == null)
                Debug.LogWarning("[EquipmentView] На infoPanel нет компонента AdaptiveInfoPanel!");
        }

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

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (e.PropertyName == nameof(ViewModel.HoveredItemData))
        {
            if (ViewModel.HoveredItemData != null)
                _infoPanel?.Show(ItemInfoFormatter.BuildContent(ViewModel.HoveredItemData), Input.mousePosition);
            else
                _infoPanel?.Hide();
        }
    }

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
        _infoPanel?.Hide();
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
