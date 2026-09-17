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

    public int Capacity { get; private set; } = 30;

    public override void Initialize()
    {
        _profile = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        BuildItemDatabase();
        Reload();
        EventBus.Subscribe(this);
    }

    public override void Cleanup()
    {
        EventBus.Unsubscribe(this);
    }

    private void BuildItemDatabase()
    {
        _itemDatabase = Resources.LoadAll<ItemData>("")
            .Where(i => !string.IsNullOrEmpty(i.itemId))
            .ToDictionary(i => i.itemId, i => i);
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
        // Например, через EventBus.RaiseEvent<IItemUseRequest>(...)
    }
}