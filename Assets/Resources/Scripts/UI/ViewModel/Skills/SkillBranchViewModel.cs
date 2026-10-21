using System.Collections.ObjectModel;
using UnityEngine;

public class SkillBranchViewModel : BaseViewModel
{
    private string _branchId;
    private string _displayName;
    private Sprite _icon;

    public string BranchId
    {
        get => _branchId;
        private set => SetProperty(ref _branchId, value);
    }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public Sprite Icon
    {
        get => _icon;
        private set => SetProperty(ref _icon, value);
    }

    public ObservableCollection<SkillNodeViewModel> Nodes { get; }

    public SkillBranchViewModel(string branchId, string displayName, Sprite icon)
    {
        _branchId = branchId;
        _displayName = displayName;
        _icon = icon;
        Nodes = new ObservableCollection<SkillNodeViewModel>();
    }

    public void AddNode(SkillNodeViewModel node)
    {
        Nodes.Add(node);
    }

    public void ClearNodes()
    {
        Nodes.Clear();
    }

    public override void Initialize()
    {

    }

    public override void Cleanup()
    {
    }
}
