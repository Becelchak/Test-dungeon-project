using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEditor.Profiling.HierarchyFrameDataView;

public class InventoryView : BaseView<InventoryViewModel>
{
    [Header("Container")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private InventoryItemSlot itemPrefab;

    [Header("Info")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI infoTitle;
    [SerializeField] private TMPro.TextMeshProUGUI infoDescription;
    [SerializeField] private Vector2 _panelOffset = new Vector3();

    private readonly List<InventoryItemSlot> _itemUIs = new();

    private RectTransform _panelRt;
    public UILayer Layer => UILayer.InventoryGroup;

    protected override void SetupBindings()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.Initialize();
        if (infoPanel != null)
        {
            _panelRt = infoPanel.GetComponent<RectTransform>();
        }
    }

    private void OnDestroy()
    {
        if (ViewModel != null) ViewModel.PropertyChanged -= OnPropertyChanged;
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (e.PropertyName == nameof(ViewModel.Items)) Rebuild();

        if (e.PropertyName == nameof(ViewModel.HoveredItemData))
        {
            UpdateInfoPanel(ViewModel.HoveredItemData);
        }

        if (e.PropertyName == nameof(ViewModel.InfoPanelPosition))
        {
            UpdateInfoPanelPosition();
        }
    }

    private void Rebuild()
    {
        Debug.Log("Rebild");
        if (_itemUIs == null)
        {
            Debug.LogWarning("Список UI пуст!");
            return;
        }

        foreach (var ui in _itemUIs) if (ui != null) Destroy(ui.gameObject);
        _itemUIs.Clear();

        if (infoPanel != null) infoPanel.SetActive(false);

        foreach (var vm in ViewModel.Items)
        {
            var ui = Instantiate(itemPrefab, itemsContainer);
            ui.Bind(vm, ViewModel);
            _itemUIs.Add(ui);
        }
    }

    public void UpdateInfoPanel(ItemData item)
    {
        if (infoPanel == null) return;

        // Если предмета нет — выключить панель
        if (item == null)
        {
            if (infoPanel.activeSelf)
            {
                infoPanel.SetActive(false);
            }
            return;
        }

        // Обновить текст только если он изменился
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

            Debug.Log("[UpdateInfoPanel] - InventoryView");
            infoPanel.SetActive(true);
        }
    }

    private void UpdateInfoPanelPosition()
    {
        if (infoPanel == null || _panelRt == null) return;
        _panelRt.position = ViewModel.InfoPanelPosition + _panelOffset;
        Debug.Log("[UpdateInfoPanelPosition] - InventoryView");
    }

}
