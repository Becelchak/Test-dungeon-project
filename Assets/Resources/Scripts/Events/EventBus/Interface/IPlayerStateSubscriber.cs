using EventBusSystem;
using UnityEngine;

public interface IPlayerStateSubscriber : IGlobalSubscriber
{
    void OnPlayerStateChanged(string stateName);
}
