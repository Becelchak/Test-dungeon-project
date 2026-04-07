using EventBusSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfileService : BaseService, IPlayerProfileService
{
    private const string PROFILE_KEY = "player_profile";
    private PlayerProfile _currentProfile;

    public PlayerProfile CurrentProfile => _currentProfile ??= LoadProfile();

    private PlayerProfile LoadProfile()
    {
        PlayerProfile profile = null;

        if (PlayerPrefs.HasKey(PROFILE_KEY))
        {
            try
            {
                var json = PlayerPrefs.GetString(PROFILE_KEY);
                profile = JsonUtility.FromJson<PlayerProfile>(json);
                Debug.Log("Профиль игрока загружен из сохранения");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка загрузки профиля: {e.Message}");
                profile = CreateDefaultProfile();
            }
        }
        else
        {
            profile = CreateDefaultProfile();
        }

        profile.LoadAvatar();
        return profile;
    }

    public void ResetProfile()
    {
        PlayerPrefs.DeleteKey(PROFILE_KEY);
        _currentProfile = null;
        var _ = CurrentProfile;
        Debug.Log("Профиль игрока сброшен и создан заново");
    }

    private PlayerProfile CreateDefaultProfile()
    {
        var profile = new PlayerProfile();

        profile.stats = new PlayerStats { firstPlayDate = DateTime.Now };

        // Рандомное выставление параметров игрока
        CreateRandomParameters(profile);

        Debug.Log("Создан новый профиль игрока по умолчанию");
        return profile;
    }

    public void CreateRandomParameters(PlayerProfile profile)
    {
        profile.level = UnityEngine.Random.RandomRange(1, 40);
        profile.health = UnityEngine.Random.RandomRange(1, 250);
        profile.maxHealth = UnityEngine.Random.RandomRange(10, 250);
        profile.mana = UnityEngine.Random.RandomRange(0, 200);
        profile.maxMana = UnityEngine.Random.RandomRange(0, 200);
        profile.strength = UnityEngine.Random.RandomRange(0, 20);
        profile.intelligence = UnityEngine.Random.RandomRange(0, 20);
        profile.agility = UnityEngine.Random.RandomRange(0, 20);

        profile.stats.goldCollected = UnityEngine.Random.RandomRange(0, 666);
        profile.stats.enemiesKilled = UnityEngine.Random.RandomRange(0, 50);
    }

    public void ModifyHealth(int delta)
    {
        var profile = CurrentProfile;
        profile.health = Mathf.Clamp(profile.health + delta, 0, profile.maxHealth);
        SaveProfile(profile);
        EventBus.RaiseEvent<IHealthChangedEventSubscriber>(
            s => s.OnHealthChanged(new HealthChangedEvent(profile.health, profile.maxHealth))
        );
    }

    public void SaveProfile(PlayerProfile profile)
    {
        try
        {
            var json = JsonUtility.ToJson(profile);
            PlayerPrefs.SetString(PROFILE_KEY, json);
            PlayerPrefs.Save();
            _currentProfile = profile;
            Debug.Log("Профиль игрока сохранен");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения профиля: {e.Message}");
        }
    }

    public void UpdatePlayerStats(Action<PlayerStats> updateAction)
    {
        updateAction?.Invoke(CurrentProfile.stats);
        SaveProfile(CurrentProfile);
    }

    public void AddInventoryItem(InventoryItem item)
    {
        var existingItem = CurrentProfile.inventory.Find(i => i.itemId == item.itemId);
        if (existingItem != null)
        {
            existingItem.quantity += item.quantity;
        }
        else
        {
            CurrentProfile.inventory.Add(item);
        }
        SaveProfile(CurrentProfile);
    }

    public void UpdateQuestProgress(string questId, QuestProgress progress)
    {
        if (CurrentProfile.quests.ContainsKey(questId))
        {
            CurrentProfile.quests[questId] = progress;
        }
        else
        {
            CurrentProfile.quests.Add(questId, progress);
        }
        SaveProfile(CurrentProfile);
    }

    protected override Type GetServiceType() => typeof(IPlayerProfileService);
}