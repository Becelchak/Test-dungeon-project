using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum SkillsTabType
{
    Common,
    Archetype
}

public class SkillsPanelViewModel : BaseViewModel
{
    private SkillsTabType _currentTab;
    private ArchetypeData _currentArchetype;

    public SkillsTabType CurrentTab
    {
        get => _currentTab;
        set
        {
            if (SetProperty(ref _currentTab, value))
            {
                OnTabChanged();
            }
        }
    }

    public ObservableCollection<SkillBranchViewModel> Branches = new ObservableCollection<SkillBranchViewModel>();

    private IProgressionService _progressionService;
    private IPlayerProfileService _profileService;
    private Dictionary<SkillBranch, SkillBranchData> _cachedBranchData = new Dictionary<SkillBranch, SkillBranchData>();

    public override void Initialize()
    {
        _progressionService = ServiceLocator.Instance.GetService<IProgressionService>();
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();

        CurrentTab = SkillsTabType.Common;

        if (_progressionService != null)
        {
            _progressionService.OnNodeUnlocked += OnNodeUnlocked;
            _progressionService.OnSkillPointsChanged += OnSkillPointsChanged;
        }

        CacheBranchData();
        LoadArchetype();
        LoadBranches();
    }

    public override void Cleanup()
    {
        if (_progressionService != null)
        {
            _progressionService.OnNodeUnlocked -= OnNodeUnlocked;
            _progressionService.OnSkillPointsChanged -= OnSkillPointsChanged;
        }
    }

    private void LoadArchetype()
    {
        if (_profileService == null) return;

        var profile = _profileService.CurrentProfile;
        if (profile == null) return;

        // Load ArchetypeData from Resources
        if (!string.IsNullOrEmpty(profile.archetypeId))
        {
            var archetypeData = Resources.Load<ArchetypeData>($"Data/ScriptableObjects/Progression/Archetypes/{profile.archetypeId}");
            if (archetypeData != null)
            {
                _currentArchetype = archetypeData;
                Debug.Log($"[SkillsPanelViewModel] Загружен архетип: {archetypeData.displayName}");
            }
            else
            {
                Debug.LogWarning($"[SkillsPanelViewModel] ArchetypeData не найден для ID: {profile.archetypeId}");
            }
        }
    }

    private void CacheBranchData()
    {
        _cachedBranchData.Clear();

        // Load from Common branches
        var commonBranches = Resources.LoadAll<SkillBranchData>("Data/ScriptableObjects/Progression/Skills/Branches/Common");
        foreach (var branch in commonBranches)
        {
            if (!_cachedBranchData.ContainsKey(branch.branch))
            {
                _cachedBranchData[branch.branch] = branch;
            }
        }

        // Load from Individual branches
        var individualBranches = Resources.LoadAll<SkillBranchData>("Data/ScriptableObjects/Progression/Skills/Branches/Individual");
        foreach (var branch in individualBranches)
        {
            if (!_cachedBranchData.ContainsKey(branch.branch))
            {
                _cachedBranchData[branch.branch] = branch;
            }
        }

        Debug.Log($"[SkillsPanelViewModel] Закешировано {_cachedBranchData.Count} веток навыков");
    }

    private void LoadBranches()
    {
        Branches.Clear();

        if (_currentTab == SkillsTabType.Common)
        {
            LoadCommonBranches();
        }
        else
        {
            LoadArchetypeBranch();
        }
    }

    private void LoadCommonBranches()
    {
        if (_currentArchetype == null) return;

        foreach (var branchData in _currentArchetype.allowedSharedBranches)
        {
            LoadBranch(branchData.branch);
        }
    }

    private void LoadArchetypeBranch()
    {
        if (_currentArchetype == null) return;

        LoadBranch(_currentArchetype.ownBranch.branch);
    }

    private void LoadBranch(SkillBranch branch)
    {
        // Get branch data from cache
        if (!_cachedBranchData.TryGetValue(branch, out var branchData))
        {
            Debug.LogWarning($"[SkillsPanelViewModel] SkillBranchData не найден в кеше для ветки: {branch}");
            return;
        }

        // Create SkillBranchViewModel
        var branchVm = new SkillBranchViewModel(branchData.branchId, branchData.displayName, branchData.icon);

        // Load SkillNodeData for this branch
        foreach (var nodeId in branchData.nodeIds)
        {
            if (_progressionService.AllSkillNodes.TryGetValue(nodeId, out var nodeData))
            {
                var nodeVm = new SkillNodeViewModel(
                    nodeData.nodeId,
                    nodeData.displayName,
                    nodeData.description,
                    nodeData.icon,
                    nodeData.cost,
                    _progressionService
                );
                branchVm.AddNode(nodeVm);
            }
            else
            {
                Debug.LogWarning($"[SkillsPanelViewModel] SkillNodeData не найден для ID: {nodeId}");
            }
        }

        Branches.Add(branchVm);
    }

    private void OnTabChanged()
    {
        LoadBranches();
    }

    private void OnNodeUnlocked(SkillNodeData node)
    {
        // Update the node's unlock status
        foreach (var branch in Branches)
        {
            //var nodeVm = branch.Nodes.FirstOrDefault(n => n.NodeId == node.nodeId);
            foreach(var nodeVm in branch.Nodes)
            {
                nodeVm?.UpdateUnlockStatus();
            }
        }
    }

    private void OnSkillPointsChanged(int newPoints)
    {
        // Update all nodes' CanUnlock status
        foreach (var branch in Branches)
        {
            foreach (var node in branch.Nodes)
            {
                node.UpdateUnlockStatus();
            }
        }
    }
}
