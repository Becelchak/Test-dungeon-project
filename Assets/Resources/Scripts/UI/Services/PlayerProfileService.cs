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

        // Загружаем аватар
        profile.LoadAvatar();
        return profile;
    }

    private PlayerProfile CreateDefaultProfile()
    {
        var profile = new PlayerProfile();

        // Начальная статистика
        profile.stats = new PlayerStats { firstPlayDate = DateTime.Now };

        Debug.Log("Создан новый профиль игрока по умолчанию");
        return profile;
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

    // Методы для обновления данных
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