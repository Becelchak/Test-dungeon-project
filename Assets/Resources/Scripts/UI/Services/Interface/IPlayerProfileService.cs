using System;
using UnityEngine;

public interface IPlayerProfileService
{
    public event Action<InventoryItem> OnItemAdded;
    public event Action<InventoryItem> OnItemRemoved;
    public PlayerProfile CurrentProfile { get; set; }
    void SaveProfile(PlayerProfile profile);
    void ResetProfile();
    void ModifyHealth(int delta);
    void ModifyStamina(float delta);
    void ModifyMana(int delta);
    void SetMaxStamina(float value);
    public void AddInventoryItem(InventoryItem item);
    public void RemoveInventoryItem(InventoryItem item);
}
