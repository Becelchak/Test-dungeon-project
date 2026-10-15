using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TriggerDatabase", menuName = "Game/Trigger Database")]
public class TriggerDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Registered Trigger")]
    [SerializeField] private List<TriggeredEffect> allTriggers = new List<TriggeredEffect>();

    private Dictionary<string, TriggeredEffect> _database = new Dictionary<string, TriggeredEffect>();

    public Dictionary<string, TriggeredEffect> Database
    {
        get
        {
            if (_database == null || _database.Count == 0)
            {
                BuildRuntimeDatabase();
            }
            return _database;
        }
    }

    [ContextMenu("Refresh Database")]
    public void RefreshDatabase()
    {
#if UNITY_EDITOR
        allTriggers.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TriggeredEffect");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TriggeredEffect trigger = UnityEditor.AssetDatabase.LoadAssetAtPath<TriggeredEffect>(path);

            if (trigger != null && !string.IsNullOrEmpty(trigger.triggerId))
            {
                allTriggers.Add(trigger);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[TriggerDatabase] База данных успешно обновлена! Найдено триггеров: {allTriggers.Count}");
#endif
    }

    private void BuildRuntimeDatabase()
    {
        _database = new Dictionary<string, TriggeredEffect>();

        foreach (var trigger in allTriggers)
        {
            if (trigger == null) continue;

            if (string.IsNullOrWhiteSpace(trigger.triggerId))
            {
                Debug.Log($"[TriggerDatabase] Триггер '{trigger.name}' пропущена, так как у неё нет traitId.");
                continue;
            }

            if (_database.ContainsKey(trigger.triggerId))
            {
                Debug.LogWarning($"[TriggerDatabase] Дубликат ID обнаружен: '{trigger.triggerId}' (Ассет: {trigger.name}). Пропущен.");
                continue;
            }

            _database.Add(trigger.triggerId, trigger);
        }

        Debug.Log($"[TriggerDatabase] Рантайм база данных успешно инициализирована. Загружено триггеров: {_database.Count}");
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {

    }
}
