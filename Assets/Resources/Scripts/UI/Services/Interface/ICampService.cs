using System.Collections.Generic;

/// <summary>
/// Сервис состояния лагеря (мета-прогрессия).
/// Хранит открытые функции/здания и ресурсы лагеря.
/// Сейчас — фундамент: Populate-логика появится вместе с контентом лагеря.
/// </summary>
public interface ICampService
{
    /// <summary>Открыта ли функция/здание лагеря по id.</summary>
    bool IsFeatureUnlocked(string featureId);
    /// <summary>Открыть функцию/здание лагеря.</summary>
    void UnlockFeature(string featureId);
    /// <summary>Количество ресурса (0, если нет).</summary>
    int GetResource(string resourceId);
    /// <summary>Изменить количество ресурса (delta может быть отрицательной).</summary>
    void AddResource(string resourceId, int delta);

    IReadOnlyCollection<string> UnlockedFeatures { get; }
    IReadOnlyDictionary<string, int> Resources { get; }
}
