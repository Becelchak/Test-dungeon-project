using NUnit.Framework;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

public class SkillNodeViewModel : BaseViewModel
{
    private string _nodeId;
    private string _displayName;
    private string _description;
    private Sprite _icon;
    private int _cost;
    private bool _isUnlocked;
    private bool _canUnlock;
    private bool _isLocked;
    private List<SkillNodeData> _requriedSkillNodes = new List<SkillNodeData>();

    public string NodeId
    {
        get => _nodeId;
        private set => SetProperty(ref _nodeId, value);
    }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public string Description
    {
        get => _description;
        private set => SetProperty(ref _description, value);
    }

    public Sprite Icon
    {
        get => _icon;
        private set => SetProperty(ref _icon, value);
    }

    public int Cost
    {
        get => _cost;
        private set => SetProperty(ref _cost, value);
    }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        private set => SetProperty(ref _isUnlocked, value);
    }

    public bool CanUnlock
    {
        get => _canUnlock;
        private set => SetProperty(ref _canUnlock, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        private set => SetProperty(ref _isLocked, value);
    }

    public List<SkillNodeData> SkillNodes
    {
        get => _requriedSkillNodes;
        private set => SetProperty(ref _requriedSkillNodes, value);
    }

    public ICommand UnlockCommand { get; private set; }

    private IProgressionService _progressionService;
    private SkillNodeData _nodeData;

    /// <summary>Лорное описание навыка (может быть пустым).</summary>
    public string LoreText => _nodeData?.loreText;

    /// <summary>Требуемый уровень прозрения для открытия навыка.</summary>
    public int MinInsightLevel => _nodeData?.minInsightLevel ?? 1;

    /// <summary>Требуемые черты персонажа (id).</summary>
    public IReadOnlyList<string> RequiredTraits => _nodeData?.requiredTraits;

    /// <summary>Разблокирован ли другой узел (для отображения требований).</summary>
    public bool IsNodeUnlocked(string nodeId) =>
        _progressionService != null && _progressionService.IsUnlocked(nodeId);

    public SkillNodeViewModel(string nodeId, string displayName, string description, Sprite icon, int cost, IProgressionService progressionService)
    {
        _nodeId = nodeId;
        _displayName = displayName;
        _description = description;
        _icon = icon;
        _cost = cost;
        _progressionService = progressionService;

        UnlockCommand = new RelayCommand(ExecuteUnlock, CanExecuteUnlock);

        _nodeData = _progressionService?.GetSkillNodeData(nodeId);

        UpdateUnlockStatus();
        FindRequiredNodes();
    }

    public void UpdateUnlockStatus()
    {
        if (_progressionService == null) return;

        IsUnlocked = _progressionService.IsUnlocked(_nodeId);
        _progressionService.CanUnlock(_nodeId, out var reason);
        CanUnlock = !IsUnlocked && (reason == UnlockReason.Ok || reason == UnlockReason.Console);
        IsLocked = !IsUnlocked && !CanUnlock;
    }

    private void FindRequiredNodes()
    {
        var skillData = _nodeData;
        if (skillData == null) return;

        foreach (var node in _progressionService.GetAllNodes())
        {
            if (skillData.requiredNodes.Contains(node.nodeId))
            {
                _requriedSkillNodes.Add(node);
            }
        }
    }

    private void ExecuteUnlock()
    {
        if (_progressionService == null) return;

        bool success = _progressionService.TryUnlock(_nodeId);
        if (success)
        {
            UpdateUnlockStatus();
        }
    }

    private bool CanExecuteUnlock()
    {
        return CanUnlock;
    }

    public override void Initialize()
    {

    }

    public override void Cleanup()
    {

    }
}
