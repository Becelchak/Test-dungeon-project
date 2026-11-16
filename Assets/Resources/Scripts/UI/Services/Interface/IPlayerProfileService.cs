using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerProfileService
{
    public event Action<InventoryItem> OnItemAdded;
    public event Action<InventoryItem> OnItemRemoved;
    public event Action OnProfileChanged;
    public PlayerProfile CurrentProfile { get; set; }
    void SaveProfile(PlayerProfile profile);
    void ResetProfile();
    void ModifyHealth(float delta);
    void ModifyStamina(float delta);
    void ModifyMana(float delta);
    void SetMaxStamina(float value);
    void AddInventoryItem(InventoryItem item);
    void RemoveInventoryItem(InventoryItem item);
    void ClearPlayerInventory();
}
