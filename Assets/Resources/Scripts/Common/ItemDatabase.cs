using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Registered Items")]
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    private Dictionary<string, ItemData> _database = new Dictionary<string, ItemData>();
    public Dictionary<string, ItemData> Database
    {
        get
        {
            // Если словарь еще не был собран в этой сессии игры — собираем его
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
        allItems.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item != null && !string.IsNullOrEmpty(item.itemId))
            {
                allItems.Add(item);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemDatabase] База данных успешно обновлена! Найдено предметов: {allItems.Count}");
#endif
    }

    /// <summary>
    /// Безопасная сборка словаря в Runtime, когда OnEnable у ассетов уже точно выполнился
    /// </summary>
    private void BuildRuntimeDatabase()
    {
        _database = new Dictionary<string, ItemData>();

        foreach (var item in allItems)
        {
            if (item == null) continue;

            if (item is WeaponData weapon)
            {
                weapon.LoadDataFromJson();
            }

            if (string.IsNullOrWhiteSpace(item.itemId))
            {
                Debug.Log($"[ItemDatabase] Предмет '{item.name}' пропущен, так как у него нет itemId.");
                continue;
            }

            if (_database.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"[ItemDatabase] Дубликат ID обнаружен: '{item.itemId}' (Ассет: {item.name}). Пропущен.");
                continue;
            }

            _database.Add(item.itemId, item);
        }

        Debug.Log($"[ItemDatabase] Рантайм база данных успешно инициализирована. Загружено предметов: {_database.Count}");
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {

    }
}
