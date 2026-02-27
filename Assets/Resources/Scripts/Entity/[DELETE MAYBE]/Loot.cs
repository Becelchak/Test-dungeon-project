using EventBusSystem;
using System;
using UnityEngine;

public class Loot : MonoBehaviour, IInteractable
{
    [SerializeField] private int goldValue = 10;
    private bool canInteract = true;

    public void Start()
    {
        var despawnable = GetComponent<Despawnable>();
        despawnable.OnDespawned += RemoveInteract;
    }

    public void Interact(GameObject interactor)
    {
        var playerProfile = (PlayerProfileService)ServiceLocator.Instance.GetService<IPlayerProfileService>();
        var playerStats = playerProfile.CurrentProfile;

        playerStats.goldCount += goldValue;
        EventBus.RaiseEvent<IGoldChangedEventSubscriber>(s => s.OnGoldChanged(new GoldChangedEvent(playerStats.goldCount)));

        GetComponent<Despawnable>().Collect();
        canInteract = false;

    }

    private void RemoveInteract(SpawnPoint point, SpawnType type)
    {
        canInteract = false;
    }

    public bool CanInteract(GameObject interactor) => canInteract;
    public string GetInteractionPrompt() => "Нажмите E, чтобы подобрать";
}