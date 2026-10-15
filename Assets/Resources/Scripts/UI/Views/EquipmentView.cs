using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EquipmentView : BaseView<EquipmentViewModel>
{
    [Header("Static Scene Slots Configuration")]
    [SerializeField] private List<EquipmentSlotUI> staticSceneSlots = new List<EquipmentSlotUI>();

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI infoTitle;
    [SerializeField] private TMPro.TextMeshProUGUI infoDescription;
    [SerializeField] private Vector2 _panelOffset = new Vector2();

    private readonly Dictionary<EquipmentSlotType, EquipmentSlotUI> _slotUIs = new();
    private RectTransform _panelRt;

    protected override void SetupBindings()
    {
        ViewModel.OnSlotsRebuilt += BindStaticSlots;
        ViewModel.OnActiveWeaponChanged += RefreshActiveHighlight;
        if (infoPanel != null)
        {
            _panelRt = infoPanel.GetComponent<RectTransform>();
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
            UpdateInfoPanel(ViewModel.HoveredItemData);
        }

        if (e.PropertyName == nameof(ViewModel.InfoPanelPosition))
        {
            UpdateInfoPanelPosition();
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
        if (infoPanel != null) infoPanel.SetActive(false);
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

    private void UpdateInfoPanel(ItemData item)
    {
        if (infoPanel == null) return;

        if (item == null)
        {
            if (infoPanel.activeSelf)
            {
                infoPanel.SetActive(false);
            }
            return;
        }

        if (infoTitle.text != item.displayName)
        {
            infoTitle.text = item.displayName;
        }
        if (infoDescription.text != item.description)
        {
            infoDescription.text = item.description;
        }

        if (!infoPanel.activeSelf)
        {
            if (_panelRt != null)
            {
                _panelRt.position = ViewModel.InfoPanelPosition + _panelOffset;
            }

            Debug.Log($"[UpdateInfoPanel] - Панель позиционирована и активирована без мигания; {ViewModel.InfoPanelPosition}");
            infoPanel.SetActive(true);
        }
    }

    private void UpdateInfoPanelPosition()
    {
        if (infoPanel == null || _panelRt == null) return;

        _panelRt.position = ViewModel.InfoPanelPosition +_panelOffset;
        Debug.Log("[UpdateInfoPanelPosition] - EquipmentView");
    }
}
