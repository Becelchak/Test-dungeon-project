using UnityEngine;

public interface IPlayerProfileService
{
    public PlayerProfile CurrentProfile { get; set; }
    void SaveProfile(PlayerProfile profile);
    void ResetProfile();
    void ModifyHealth(int delta);
}
