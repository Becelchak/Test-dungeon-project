using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сохраняемое состояние лагеря: открытые функции и ресурсы.
/// </summary>
[Serializable]
public class CampState
{
    public List<string> unlockedFeatures = new List<string>();
    public Dictionary<string, int> resources = new Dictionary<string, int>();
}

/// <summary>
/// Сервис состояния лагеря — ISaveModule (секция "camp").
/// Фундамент для мета-прогрессии: сюда будут писаться разблокировки
/// зданий/функций и ресурсы по мере появления контента лагеря.
/// </summary>
public class CampService : BaseService, ICampService, ISaveModule
{
    private CampState _state = new CampState();
    private ISaveService _saveService;
    private ISaveService SaveService => _saveService ??= ServiceLocator.Instance.GetService<ISaveService>();

    public IReadOnlyCollection<string> UnlockedFeatures => _state.unlockedFeatures;
    public IReadOnlyDictionary<string, int> Resources => _state.resources;

    protected override void Awake()
    {
        base.Awake();
        SaveService?.RegisterModule(this);
    }

    public string Key => "camp";

    public bool IsFeatureUnlocked(string featureId)
    {
        return !string.IsNullOrEmpty(featureId) && _state.unlockedFeatures.Contains(featureId);
    }

    public void UnlockFeature(string featureId)
    {
        if (string.IsNullOrEmpty(featureId) || IsFeatureUnlocked(featureId)) return;
        _state.unlockedFeatures.Add(featureId);
        SaveService?.RequestSave();
        Debug.Log($"[Camp] Открыта функция: {featureId}");
    }

    public int GetResource(string resourceId)
    {
        if (string.IsNullOrEmpty(resourceId)) return 0;
        return _state.resources.TryGetValue(resourceId, out var value) ? value : 0;
    }

    public void AddResource(string resourceId, int delta)
    {
        if (string.IsNullOrEmpty(resourceId) || delta == 0) return;

        _state.resources.TryGetValue(resourceId, out var current);
        _state.resources[resourceId] = Mathf.Max(0, current + delta);
        SaveService?.RequestSave();
    }

    public string CaptureStateJson()
    {
        // Newtonsoft.Json: Dictionary<string,int> не поддерживается JsonUtility
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreStateJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var state = JsonConvert.DeserializeObject<CampState>(json);
            if (state != null)
                _state = state;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Camp] Ошибка восстановления состояния лагеря: {e.Message}");
        }
    }

    protected override Type GetServiceType() => typeof(ICampService);
}
