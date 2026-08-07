using EventBusSystem;

/// <summary>
/// Событие подбора предмета игроком.
/// </summary>
public class ItemPickedUpEvent
{
    public ItemData Item { get; }
    public int Quantity { get; }

    public ItemPickedUpEvent(ItemData item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

public interface IItemPickedUpEventSubscriber : IGlobalSubscriber
{
    void OnItemPickedUp(ItemPickedUpEvent evt);
}
