using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class InventoryView : BaseView<InventoryViewModel>
{
    [Header("Container")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private InventoryItemSlot itemPrefab;

    [Header("Info")]
    [Tooltip("GameObject с компонентом ItemInfoPanel")]
    [SerializeField] private GameObject infoPanel;

    private readonly List<InventoryItemSlot> _itemUIs = new();
    private ItemInfoPanel _infoPanel;

    public UILayer Layer => UILayer.InventoryGroup;

    protected override void SetupBindings()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.Initialize();
        if (infoPanel != null)
        {
            _infoPanel = infoPanel.GetComponent<ItemInfoPanel>();
            if (_infoPanel == null)
                Debug.LogWarning("[InventoryView] На infoPanel нет компонента ItemInfoPanel!");
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
            if (ViewModel.HoveredItemData != null)
                _infoPanel?.Show(ViewModel.HoveredItemData, Input.mousePosition);
            else
                _infoPanel?.Hide();
        }
    }

    private void Rebuild()
    {
        if (_itemUIs == null)
        {
            Debug.LogWarning("Список UI пуст!");
            return;
        }

        foreach (var ui in _itemUIs) if (ui != null) Destroy(ui.gameObject);
        _itemUIs.Clear();

        _infoPanel?.Hide();

        foreach (var vm in ViewModel.Items)
        {
            var ui = Instantiate(itemPrefab, itemsContainer);
            ui.Bind(vm, ViewModel);
            _itemUIs.Add(ui);
        }
    }
}
