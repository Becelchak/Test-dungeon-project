using System;
using UnityEngine;

public class PlayerProfileService
{
    private const string PROFILE_KEY = "player_profile";
    private PlayerProfile _currentProfile;

    public PlayerProfile CurrentProfile => _currentProfile ??= LoadProfile();

    private PlayerProfile LoadProfile()
    {
        if (PlayerPrefs.HasKey(PROFILE_KEY))
        {
            var json = PlayerPrefs.GetString(PROFILE_KEY);
            return JsonUtility.FromJson<PlayerProfile>(json);
        }

        // Создание профиля по умолчанию
        return new PlayerProfile();
    }

    public void SaveProfile(PlayerProfile profile)
    {
        var json = JsonUtility.ToJson(profile);
        PlayerPrefs.SetString(PROFILE_KEY, json);
        PlayerPrefs.Save();
        _currentProfile = profile;
    }
}

