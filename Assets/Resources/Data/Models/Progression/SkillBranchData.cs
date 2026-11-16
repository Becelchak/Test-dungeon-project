using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/SkillBranchData")]
public class SkillBranchData : ScriptableObject
{
    public string branchId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillBranch branch;
    public List<string> nodeIds;

    [ContextMenu("Refresh Branch Data")]
    public void RefreshDatabase()
    {
#if UNITY_EDITOR
        nodeIds.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SkillNodeData");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            SkillNodeData node = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);

            if (node != null && !string.IsNullOrEmpty(node.nodeId) && branch == node.branch)
            {
                nodeIds.Add(node.nodeId);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SkillBranchData] Ветка {displayName} успешно обновлена! Найдено навыков: {nodeIds.Count}");
#endif
    }
}
