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

    private readonly List<InventoryItemSlot> _itemUIs = new();

    public UILayer Layer => UILayer.InventoryGroup;

    protected override void SetupBindings()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.Initialize();
        //Rebuild();
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

        //Vector2 mousePos = Input.mousePosition;
        //RectTransform rt = infoPanel.GetComponent<RectTransform>();

        //if (infoPanel.activeSelf)
        //{
        //    Vector2 targetPosition = mousePos + new Vector2(20f, -20f);

        //    if ((Vector2)rt.position != targetPosition)
        //    {
        //        rt.position = targetPosition;
        //    }
        //}

        if (item == null)
        {
            infoPanel.SetActive(false);
            return;
        }

        infoPanel.SetActive(true);
        infoTitle.text = item.displayName;
        infoDescription.text = item.description;
    }
}