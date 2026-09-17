using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image background;
    [SerializeField] private GameObject selectionHighlight;

    private InventoryItemSlotViewModel _vm;
    private InventoryViewModel _inventoryVm;
    private CanvasGroup _canvasGroup;
    private RectTransform _ghost;
    private Canvas _rootCanvas;

    public InventoryItemSlotViewModel ViewModel => _vm;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(InventoryItemSlotViewModel vm, InventoryViewModel inventoryVm)
    {
        _vm = vm;
        _inventoryVm = inventoryVm;
        _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        Refresh();
    }

    private void Refresh()
    {
        if (_vm == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = _vm.Icon;
            iconImage.enabled = _vm.Icon != null;
        }
        if (nameText != null) nameText.text = _vm.DisplayName;
        if (quantityText != null)
        {
            quantityText.text = _vm.IsStackable ? _vm.Quantity.ToString() : "";
            quantityText.gameObject.SetActive(_vm.IsStackable);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null) selectionHighlight.SetActive(selected);
    }


    public void OnBeginDrag(PointerEventData e)
    {
        if (_vm == null) return;
        if (_rootCanvas == null) _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        _canvasGroup.blocksRaycasts = false;

        var ghostGO = new GameObject("InventoryDragGhost",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        _ghost = ghostGO.GetComponent<RectTransform>();
        _ghost.SetParent(_rootCanvas.transform, false);
        _ghost.sizeDelta = new Vector2(64, 64);
        _ghost.SetAsLastSibling();

        var img = ghostGO.GetComponent<Image>();
        img.sprite = _vm.Icon;
        img.raycastTarget = false;
        img.preserveAspect = true;

        ItemDragPayload.Current = new ItemDragPayloadData(_vm, _inventoryVm);
    }

    public void OnDrag(PointerEventData e)
    {
        if (_ghost == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.transform as RectTransform,
            e.position, _rootCanvas.worldCamera, out var p);
        _ghost.localPosition = p;
    }

    public void OnEndDrag(PointerEventData e)
    {
        _canvasGroup.blocksRaycasts = true;
        if (_ghost != null) Destroy(_ghost.gameObject);
        ItemDragPayload.Current = null;
    }

    public void OnPointerClick(PointerEventData e)
    {
        // Действие при клике на иконку
    }
}

/// <summary>Глобальная «булавка» — что сейчас тащат.</summary>
public static class ItemDragPayload
{
    public static ItemDragPayloadData Current { get; set; }
}

public class ItemDragPayloadData
{
    public InventoryItemSlotViewModel ItemVm;
    public InventoryViewModel Source;

    public ItemDragPayloadData(InventoryItemSlotViewModel itemVm, InventoryViewModel source)
    {
        ItemVm = itemVm; 
        Source = source;
    }
}