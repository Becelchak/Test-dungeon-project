using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.ComponentModel;

public class SkillsPanelView : BaseView<SkillsPanelViewModel>
{
    [Header("Tab Buttons")]
    public Button commonTabButton;
    public Button archetypeTabButton;

    [Header("Panels")]
    public CanvasGroup commonBranchesPanel;
    public CanvasGroup archetypeBranchPanel;

    [Header("Branch Views (Manual Setup)")]
    public List<SkillBranchView> commonBranchViews;
    public SkillBranchView archetypeBranchView;

    [Header("Info Panel")]
    [Tooltip("Адаптивная панель информации о навыке (AdaptiveInfoPanel)")]
    [SerializeField] private AdaptiveInfoPanel infoPanel;

    public UILayer Layer => UILayer.Skills;

    private readonly List<SkillNodeView> _nodeViews = new List<SkillNodeView>();

    protected override void SetupBindings()
    {
        if (commonTabButton != null)
            commonTabButton.onClick.AddListener(OnCommonTabClick);

        if (archetypeTabButton != null)
            archetypeTabButton.onClick.AddListener(OnArchetypeTabClick);

        ViewModel.PropertyChanged += OnPropertyChanged;
        RefreshTab();

        if (infoPanel != null)
        {
            _nodeViews.AddRange(GetComponentsInChildren<SkillNodeView>(true));
            foreach (var nodeView in _nodeViews)
            {
                nodeView.Hovered += OnNodeHovered;
                nodeView.Unhovered += OnNodeUnhovered;
            }
        }
    }

    public override void Unbind()
    {
        foreach (var nodeView in _nodeViews)
        {
            if (nodeView == null) continue;
            nodeView.Hovered -= OnNodeHovered;
            nodeView.Unhovered -= OnNodeUnhovered;
        }
        _nodeViews.Clear();
        infoPanel?.Hide();

        base.Unbind();
    }

    private void OnNodeHovered(SkillNodeViewModel node)
    {
        infoPanel?.Show(SkillInfoFormatter.Build(node), Input.mousePosition);
    }

    private void OnNodeUnhovered()
    {
        infoPanel?.Hide();
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SkillsPanelViewModel.CurrentTab))
        {
            RefreshTab();
        }
    }

    private void RefreshTab()
    {
        if (ViewModel == null) return;

        bool showCommon = ViewModel.CurrentTab == SkillsTabType.Common;

        if (commonBranchesPanel != null)
        {
            commonBranchesPanel.interactable = showCommon;
            commonBranchesPanel.blocksRaycasts = showCommon;
            commonBranchesPanel.alpha = showCommon ? 1 : 0;
        }

        if (archetypeBranchPanel != null)
            archetypeBranchPanel.interactable = !showCommon;
        archetypeBranchPanel.blocksRaycasts = !showCommon;
        archetypeBranchPanel.alpha = !showCommon ? 1: 0;

        if (showCommon)
        {
            BindCommonBranches();
        }
        else
        {
            BindArchetypeBranch();
        }
    }

    private void BindCommonBranches()
    {
        if (ViewModel == null || commonBranchViews == null) return;

        for (int i = 0; i < commonBranchViews.Count && i < ViewModel.Branches.Count; i++)
        {
            commonBranchViews[i].Bind(ViewModel.Branches[i]);
        }
    }

    private void BindArchetypeBranch()
    {
        if (ViewModel == null || archetypeBranchView == null) return;

        if (ViewModel.Branches.Count > 0)
        {
            archetypeBranchView.Bind(ViewModel.Branches[0]);
        }
    }

    private void OnCommonTabClick()
    {
        if (ViewModel != null)
        {
            ViewModel.CurrentTab = SkillsTabType.Common;
        }
    }

    private void OnArchetypeTabClick()
    {
        if (ViewModel != null)
        {
            ViewModel.CurrentTab = SkillsTabType.Archetype;
        }
    }
}
