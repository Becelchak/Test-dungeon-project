using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Registered Items")]
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();

    private Dictionary<string, ItemData> _database = new Dictionary<string, ItemData>();
    public Dictionary<string, ItemData> Database => _database;

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

    public void OnAfterDeserialize()
    {
        _database.Clear();
        foreach (var item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId)) continue;

            if (_database.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"[ItemDatabase] Дубликат ID обнаружен: {item.itemId}. Пропущен.");
                continue;
            }
            _database.Add(item.itemId, item);
        }
    }

    public void OnBeforeSerialize() { }
}
