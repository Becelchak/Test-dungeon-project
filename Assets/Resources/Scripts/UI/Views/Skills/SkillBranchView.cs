using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using System.Net;

public class SkillBranchView : MonoBehaviour
{
    [Header("Branch Info")]
    public string branchId;
    public TextMeshProUGUI branchNameText;
    public Image branchIconImage;
    public UILineRenderer globalLineRenderer;

    [Header("Nodes Container")]
    [SerializeField] private Transform nodesContainer;
    [SerializeField] float offset = 28.5f;
    private SkillBranchViewModel _viewModel;
    private Dictionary<string, SkillNodeView> _nodeViews = new Dictionary<string, SkillNodeView>();
    private RectTransform localCanvasRect;

    public void Bind(SkillBranchViewModel viewModel)
    {
        _viewModel = viewModel;
        localCanvasRect = globalLineRenderer.GetComponent<RectTransform>();

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
        List<UILineRenderer.LineSegment> allSegments = new List<UILineRenderer.LineSegment>();
        foreach (var nodeView in _nodeViews.Values)
        {
            var nodesForConnection = nodeView.GetRequiredNodes();
            foreach (var node in nodesForConnection)
            {
                CreateLineBetween(nodeView, _nodeViews[node.nodeId], ref allSegments);
            }
        }
        globalLineRenderer.SetSegments(allSegments);
    }

    private void CreateLineBetween(SkillNodeView parent, SkillNodeView child, ref List<UILineRenderer.LineSegment> allSegments)
    {

        var parentRect = parent.GetComponent<RectTransform>();
        var childRect = child.GetComponent<RectTransform>();

        Vector3 parentWorld = parentRect.TransformPoint(Vector2.zero);
        Vector3 childWorld = childRect.TransformPoint(Vector2.zero);

        Vector2 startPos = localCanvasRect.InverseTransformPoint(parentWorld);
        Vector2 endPos = localCanvasRect.InverseTransformPoint(childWorld);


        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        if (distance > offset)
        {
            startPos += direction * offset;
            endPos -= direction * offset;
        }

        allSegments.Add(new UILineRenderer.LineSegment(startPos, endPos));
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
