using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image background;
    [SerializeField] private GameObject selectionHighlight;

    [SerializeField] private InventoryItemSlotViewModel _vm;
    private InventoryViewModel _inventoryVm;
    private CanvasGroup _canvasGroup;
    private RectTransform _ghost;
    private Canvas _rootCanvas;
    private IWindowService _windowService;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        if (_ghost != null) Destroy(_ghost.gameObject);
    }

    public void Bind(InventoryItemSlotViewModel vm, InventoryViewModel inventoryVm)
    {
        _vm = vm;
        _inventoryVm = inventoryVm;
        _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        _windowService = ServiceLocator.Instance.GetService<IWindowService>();
        Refresh();
    }

    private void Refresh()
    {
        if (_vm == null) return;

        if (iconImage != null && _vm.Icon != null)
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
        if (_vm == null || _vm.Data == null) return;

        if (e.button == PointerEventData.InputButton.Left)
        {
            ShowContextMenu();
        }
    }

    private void ShowContextMenu()
    {
        if (_windowService == null) return;

        var contextMenuObj = _windowService.ShowWindow<ContextMenuViewModel>(UILayer.ContextMenu);
        if (contextMenuObj != null)
        {
            var contextMenuView = contextMenuObj.GetComponentInChildren<ContextMenuView>();
            if (contextMenuView != null)
            {
                var viewModel = contextMenuView.viewModel as ContextMenuViewModel;
                viewModel?.Show(_vm, _inventoryVm);

                // Позиционируем меню у курсора
                RectTransform menuRect = contextMenuObj.GetComponent<RectTransform>();
                if (menuRect != null)
                {
                    Vector2 mousePos = Input.mousePosition;
                    menuRect.position = mousePos;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_vm?.Data != null && _inventoryVm != null)
        {
            _inventoryVm.SetHoveredItem(_vm.Data);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _inventoryVm?.ClearHoveredItem();
    }
}
