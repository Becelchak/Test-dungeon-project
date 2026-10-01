using UnityEngine;

public class DropItemAction : IItemAction
{
    public string ActionName => "Выбросить";
    public string ActionDescription => "Удалить предмет из инвентаря";

    public bool CanExecute(ItemData item)
    {
        return item != null;
    }

    public void Execute(ItemData item, InventoryItemSlotViewModel itemVm)
    {
        if (itemVm == null || itemVm.RuntimeItem == null) return;

        var profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        if (profileService != null)
        {
            profileService.RemoveInventoryItem(itemVm.RuntimeItem);
            Debug.Log($"[DropItemAction] Предмет {item.displayName} выброшен из инвентаря");
        }
    }
}
