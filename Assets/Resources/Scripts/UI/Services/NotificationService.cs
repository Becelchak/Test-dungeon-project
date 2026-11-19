using EventBusSystem;
using System;
using UnityEngine;

public class NotificationService : BaseService, INotificationService,
    IXPChangedEventSubscriber, ILevelUpEventSubscriber, IInventoryChangedEventSubscriber
{
    protected override Type GetServiceType() => typeof(INotificationService);

    private void Start()
    {
        EventBus.Subscribe(this);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(this);
        base.OnDestroy();
    }

    public void Notify(NotificationType type, string title, string message)
    {
        EventBus.RaiseEvent<INotificationAddedEventSubscriber>(s => s.OnNotificationAdded(
            new NotificationAddedEvent(new NotificationData(type, title, message))));
    }

    public void OnXPChanged(XPChangedEvent evt)
    {
        Notify(NotificationType.XP, "Опыт", $"Вы стали умнее на {evt.AmountGained} очков");
    }

    public void OnLevelUp(LevelUpEvent evt)
    {
        Notify(NotificationType.LevelUp, "Повышение уровня",
            $"Вы достигли {evt.NewLevel} уровня! +{evt.SkillPointsGranted} очко навыков");
    }

    public void OnInventoryChanged(InventoryChangedEvent evt)
    {
        switch (evt.Type)
        {
            case InventoryChangedEvent.ChangeType.Added:
                Notify(NotificationType.Info, "Предмет подобран",
                $"{evt.Item.itemName}");
                break;
        }
        
    }
}
