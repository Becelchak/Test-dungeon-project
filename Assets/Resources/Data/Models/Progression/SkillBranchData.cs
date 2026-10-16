using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/SkillBranchData")]
public class SkillBranchData : ScriptableObject
{
    public string branchId;
    public string displayName;
    [TextArea] public string description;
    public SkillBranch branch;
    public List<string> nodeIds;
}
