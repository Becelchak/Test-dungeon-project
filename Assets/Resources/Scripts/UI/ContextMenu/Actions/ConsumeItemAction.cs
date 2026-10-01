using UnityEngine;

public class ConsumeItemAction : IItemAction
{
    public string ActionName => "Использовать";
    public string ActionDescription => "Использовать предмет";

    public bool CanExecute(ItemData item)
    {
        return item != null && item.itemType == ItemType.Consumable;
    }

    public void Execute(ItemData item, InventoryItemSlotViewModel itemVm)
    {
        if (itemVm == null || itemVm.RuntimeItem == null) return;

        var profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        if (profileService != null)
        {
            profileService.RemoveInventoryItem(itemVm.RuntimeItem);
            Debug.Log($"[ConsumeItemAction] Предмет {item.displayName} использован");

            // TODO: Добавить логику применения эффектов предмета
            // Например: восстановление здоровья, маны и т.д.
        }
    }
}
