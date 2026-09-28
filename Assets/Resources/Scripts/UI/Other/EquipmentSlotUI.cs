using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EventBusSystem;

public class EquipmentSlotUI : MonoBehaviour,
    IDropHandler, IPointerClickHandler
{
    [Header("Slot Identity")]
    [SerializeField] private EquipmentSlotType slotType;
    public EquipmentSlotType SlotType => slotType;

    [Header("UI Renderers")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image placeholderImage;
    [SerializeField] private GameObject activeHighlight;
    [SerializeField] private GameObject lockedOverlay;

    private EquipmentSlotViewModel _slotVm;
    private EquipmentViewModel _equipmentVm;
    private CanvasGroup _canvasGroup;
    private IInputService _inputService;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(EquipmentSlotViewModel slotVm, EquipmentViewModel equipmentVm)
    {
        _slotVm = slotVm;
        _equipmentVm = equipmentVm;
        _slotVm.OnChanged += Refresh;
        _inputService = ServiceLocator.Instance.GetService<IInputService>();
        Refresh();
    }

    private void OnDestroy()
    {
        if (_slotVm != null) _slotVm.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        if (_slotVm == null) return;

        if (_slotVm.Item != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = _slotVm.Icon;
                iconImage.enabled = true;
            }
            if (placeholderImage != null) placeholderImage.enabled = false;
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (placeholderImage != null) placeholderImage.enabled = true;
        }

        if (lockedOverlay != null) lockedOverlay.SetActive(!_slotVm.IsUnlocked);
    }

    public void SetActiveHighlight(bool active)
    {
        if (activeHighlight != null) activeHighlight.SetActive(active);
    }

    /// <summary>
    /// Приём предмета ИЗ инвентаря
    /// </summary>
    /// <param name="e"></param>
    public void OnDrop(PointerEventData e)
    {
        var payloadData = ItemDragPayload.Current;

        if (payloadData == null || ItemDragPayload.Source != DragSourceType.Inventory || payloadData.ItemVm == null) return;

        EventBus.RaiseEvent<IEquipmentRequestSubscriber>(s =>
            s.OnRequestEquipFromInventory(payloadData.ItemVm.Data, this.slotType));
    }

    /// <summary>
    /// ОБРАБОТКА КЛИКОВ
    /// </summary>
    /// <param name="e"></param>
    public void OnPointerClick(PointerEventData e)
    {
        if (_slotVm == null) return;

        if (_inputService != null && _inputService.IsUnequipPressed)
        {
            // Если в момент клика по ячейке новая Input System видит нажатие кнопки Cancel (ПКМ/Escape)
            if (_slotVm.Item != null)
            {
                var itemInventory = new InventoryItem
                {
                    itemId = _slotVm.Item.itemId,
                    itemName = _slotVm.Item.name,
                    description = _slotVm.Item.description,
                    quantity = 1,
                    type = _slotVm.Item.itemType,
                };
                Debug.Log($"[EquipmentSlotUI] Снятие предмета {this.slotType} через новую Input System.");
                EventBus.RaiseEvent<IEquipmentRequestSubscriber>(s =>
                    s.OnRequestUnequipToInventory(itemInventory, this.slotType));
            }
            return;
        }

        // 2. Обычное нажатие (ЛКМ) для выбора активного оружия
        if (_slotVm.SlotType == EquipmentSlotType.Weapon1 ||
            _slotVm.SlotType == EquipmentSlotType.Weapon2 ||
            _slotVm.SlotType == EquipmentSlotType.Weapon3)
        {
            int index = _slotVm.SlotType - EquipmentSlotType.Weapon1;
            _equipmentVm.SetActiveWeaponSlot(index);
        }
    }
}
