using EventBusSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryViewModel : BaseViewModel, IInventoryChangedEventSubscriber
{
    private IPlayerProfileService _profile;
    private Dictionary<string, ItemData> _itemDatabase;

    public IReadOnlyList<InventoryItemSlotViewModel> Items => _items;
    private readonly List<InventoryItemSlotViewModel> _items = new();

    private ItemData _hoveredItemData;
    private Vector2 _infoPanelPosition;

    public ItemData HoveredItemData
    {
        get => _hoveredItemData;
        private set
        {
            _hoveredItemData = value;
            OnPropertyChanged(nameof(HoveredItemData));
        }
    }

    public Vector2 InfoPanelPosition
    {
        get => _infoPanelPosition;
        private set
        {
            if (_infoPanelPosition == value) return;
            _infoPanelPosition = value;
            OnPropertyChanged(nameof(InfoPanelPosition));
        }
    }


    public int Capacity { get; private set; } = 30;

    public override void Initialize()
    {
        _profile = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _itemDatabase = ServiceLocator.Instance.GetService<IResourceService>().GetItemDataBase();
        Reload();
        EventBus.Subscribe(this);
    }

    public override void Cleanup()
    {
        EventBus.Unsubscribe(this);
    }

    public void Reload()
    {
        _items.Clear();
        foreach (var runtimeItem in _profile.CurrentProfile.inventory)
        {
            _itemDatabase.TryGetValue(runtimeItem.itemId, out var data);
            _items.Add(new InventoryItemSlotViewModel(runtimeItem, data));
        }
        OnPropertyChanged(nameof(Items));
    }

    public void OnInventoryChanged(InventoryChangedEvent evt) => Reload();

    /// <summary>Снять/убрать предмет из сумки (например, после экипировки).</summary>
    public void RequestRemove(InventoryItemSlotViewModel vm)
    {
        if (vm?.RuntimeItem == null) return;
        _profile.RemoveInventoryItem(vm.RuntimeItem);
    }

    /// <summary>Использовать предмет (например, выпить зелье).</summary>
    public void RequestUse(InventoryItemSlotViewModel vm)
    {
        if (vm?.Data == null) return;
        // Логика использования — за пределами этого ViewModel.
    }

    /// <summary>Установить активный предмет для показа описания.</summary>
    public void SetHoveredItem(ItemData data)
    {
        if (_hoveredItemData == data) return;

        HoveredItemData = data;

        if (data != null)
        {
            InfoPanelPosition = Input.mousePosition + new Vector3(20f, -20f);
        }
    }

    /// <summary>Очистить инфо-панель, когда курсор убран.</summary>
    public void ClearHoveredItem()
    {
        HoveredItemData = null;
    }

    /// <summary>Быстрая экипировка предмета (через RightClick).</summary>
    public void RequestQuickEquip(InventoryItemSlotViewModel vm)
    {
        if (vm?.Data == null) return;

        Debug.Log($"[InventoryViewModel] RequestQuickEquip: {vm.Data.displayName}");

        var equipAction = new EquipItemAction();
        if (equipAction.CanExecute(vm.Data))
        {
            equipAction.Execute(vm.Data, vm);
            Reload();
        }
        else
        {
            Debug.LogWarning($"[InventoryViewModel] Нельзя экипировать {vm.Data.displayName}");
        }
    }
}
