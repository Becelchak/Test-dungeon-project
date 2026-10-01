using UnityEngine;

public interface IItemAction
{
    string ActionName { get; }
    string ActionDescription { get; }
    bool CanExecute(ItemData item);
    void Execute(ItemData item, InventoryItemSlotViewModel itemVm);
}
