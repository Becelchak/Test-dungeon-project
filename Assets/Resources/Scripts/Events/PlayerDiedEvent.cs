using EventBusSystem;
using UnityEngine;

/// <summary>
/// Событие смерти игрока.
/// </summary>
public struct PlayerDiedEvent
{
    public Vector3 Position;

    public PlayerDiedEvent(Vector3 position)
    {
        Position = position;
    }
}

public interface IPlayerDiedEventSubscriber : IGlobalSubscriber
{
    void OnPlayerDied(PlayerDiedEvent evt);
}
