using EventBusSystem;
using UnityEngine;

public interface IRespawnIntervalChangedEventSubscriber : IGlobalSubscriber
{
    void OnShowNotification(RespawnIntervalChangedEvent evt);
}
