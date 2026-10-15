using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraitDatabase", menuName = "Game/Trait Database")]
public class TraitDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Registered Traits")]
    [SerializeField] private List<TraitData> allTraits = new List<TraitData>();

    private Dictionary<string, TraitData> _database = new Dictionary<string, TraitData>();

    public Dictionary<string, TraitData> Database
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
        allTraits.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TraitData");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TraitData trait = UnityEditor.AssetDatabase.LoadAssetAtPath<TraitData>(path);

            if (trait != null && !string.IsNullOrEmpty(trait.traitId))
            {
                allTraits.Add(trait);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[TraitDatabase] База данных успешно обновлена! Найдено особенностей: {allTraits.Count}");
#endif
    }

    private void BuildRuntimeDatabase()
    {
        _database = new Dictionary<string, TraitData>();

        foreach (var trait in allTraits)
        {
            if (trait == null) continue;

            if (string.IsNullOrWhiteSpace(trait.traitId))
            {
                Debug.Log($"[TraitDatabase] Особенность '{trait.name}' пропущена, так как у неё нет traitId.");
                continue;
            }

            if (_database.ContainsKey(trait.traitId))
            {
                Debug.LogWarning($"[TraitDatabase] Дубликат ID обнаружен: '{trait.traitId}' (Ассет: {trait.name}). Пропущен.");
                continue;
            }

            _database.Add(trait.traitId, trait);
        }

        Debug.Log($"[TraitDatabase] Рантайм база данных успешно инициализирована. Загружено особенностей: {_database.Count}");
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {

    }
}
