using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Game/Skill Database")]
public class SkillDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Registered Skills")]
    [SerializeField] private List<SkillNodeData> allSkills = new List<SkillNodeData>();

    private Dictionary<string, SkillNodeData> _database = new Dictionary<string, SkillNodeData>();

    public Dictionary<string, SkillNodeData> Database
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
        allSkills.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SkillNodeData");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var skill = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);

            if (skill != null && !string.IsNullOrEmpty(skill.nodeId))
            {
                allSkills.Add(skill);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[TraitDatabase] База данных успешно обновлена! Найдено особенностей: {allSkills.Count}");
#endif
    }

    private void BuildRuntimeDatabase()
    {
        _database = new Dictionary<string, SkillNodeData>();

        foreach (var skill in allSkills)
        {
            if (skill == null) continue;

            if (string.IsNullOrWhiteSpace(skill.nodeId))
            {
                Debug.Log($"[TraitDatabase] Особенность '{skill.name}' пропущена, так как у неё нет traitId.");
                continue;
            }

            if (_database.ContainsKey(skill.nodeId))
            {
                Debug.LogWarning($"[TraitDatabase] Дубликат ID обнаружен: '{skill.nodeId}' (Ассет: {skill.name}). Пропущен.");
                continue;
            }

            _database.Add(skill.nodeId, skill);
        }

        Debug.Log($"[TraitDatabase] Рантайм база данных успешно инициализирована. Загружено особенностей: {_database.Count}");
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {

    }
}
