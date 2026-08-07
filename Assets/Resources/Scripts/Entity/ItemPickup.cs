using EventBusSystem;
using System.Linq;
using UnityEngine;

/// <summary>
/// Лежащий в мире предмет, который игрок может подобрать кнопкой взаимодействия.
/// </summary>
public class ItemPickup : MonoBehaviour, IInteractable
{
    [Tooltip("Предмет, который будет добавлен в инвентарь")]
    [SerializeField] private ItemData item;

    [Tooltip("Количество предметов за одно взаимодействие")]
    [SerializeField] private int quantity = 1;

    [Tooltip("Удалить объект после подбора?")]
    [SerializeField] private bool destroyOnPickup = true;

    private bool _canInteract = true;

    public void Interact(GameObject interactor)
    {
        if (!_canInteract || item == null) return;

        var profileService = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
        if (profileService == null)
        {
            Debug.LogError("[ItemPickup] Не найден PlayerProfileService!");
            return;
        }

        var inventoryItem = new InventoryItem
        {
            itemId = item.itemId,
            itemName = item.displayName,
            description = item.description,
            quantity = quantity,
            type = item.itemType
        };

        profileService.AddInventoryItem(inventoryItem);
        LogInventory(profileService.CurrentProfile);

        // Проигрываем анимацию подбора поверх текущего состояния
        var animController = interactor.GetComponentInChildren<PlayerAnimationController>();
        if (animController != null)
            animController.TriggerPickup();

        EventBus.RaiseEvent<IItemPickedUpEventSubscriber>(
            s => s.OnItemPickedUp(new ItemPickedUpEvent(item, quantity))
        );

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            _canInteract = false;
    }

    private void LogInventory(PlayerProfile profile)
    {
        var summary = profile.inventory.Count == 0
            ? "пуст"
            : string.Join(", ", profile.inventory.Select(i => $"{i.itemName} x{i.quantity}"));
        Debug.Log($"[Inventory] Текущий инвентарь: {summary}");
    }

    public bool CanInteract(GameObject interactor) => _canInteract && item != null;

    public string GetInteractionPrompt() => item != null ? $"Подобрать {item.displayName}" : "Подобрать";
}
