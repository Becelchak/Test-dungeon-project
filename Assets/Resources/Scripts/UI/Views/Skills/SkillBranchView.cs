using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class SkillBranchView : MonoBehaviour
{
    [Header("Branch Info")]
    public string branchId;
    public TextMeshProUGUI branchNameText;
    public Image branchIconImage;

    [Header("Nodes Container")]
    public Transform nodesContainer;

    private SkillBranchViewModel _viewModel;
    private Dictionary<string, SkillNodeView> _nodeViews = new Dictionary<string, SkillNodeView>();

    public void Bind(SkillBranchViewModel viewModel)
    {
        _viewModel = viewModel;

        if (branchNameText != null)
            branchNameText.text = viewModel.DisplayName;

        if (branchIconImage != null)
            branchIconImage.sprite = viewModel.Icon;

        SpawnSkillNodes();
    }

    public void Unbind()
    {
        _viewModel = null;
        ClearNodes();
    }

    private void SpawnSkillNodes()
    {
        if (_viewModel == null || nodesContainer == null) return;

        var existingNodeViews = nodesContainer.GetComponentsInChildren<SkillNodeView>();

        foreach (var nodeView in existingNodeViews)
        {
            var nodeVm = _viewModel.Nodes.FirstOrDefault(n => n.NodeId == nodeView.nodeId);
            if (nodeVm != null)
            {
                nodeView.Bind(nodeVm);
                _nodeViews[nodeVm.NodeId] = nodeView;
            }
        }

        DrawConnections();
    }

    private void ClearNodes()
    {
        foreach (var nodeView in _nodeViews.Values)
        {
            if (nodeView != null)
            {
                nodeView.Unbind();
            }
        }
        _nodeViews.Clear();
    }

    private void DrawConnections()
    {
        // TODO: Implement line drawing between nodes
        // Need use SkillNodeData.requiredNodes to determine connections
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
