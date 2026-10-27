using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : Graphic
{
    public float thickness = 5f;
    [System.Serializable]
    public struct LineSegment
    {
        public Vector2 start;
        public Vector2 end;

        public LineSegment(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }
    }
    [SerializeField] private List<LineSegment> segments = new List<LineSegment>();

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Полностью перезаписывает все линии в компоненте
    /// </summary>
    public void SetSegments(List<LineSegment> newSegments)
    {
        segments = new List<LineSegment>(newSegments);
        SetAllDirty();
    }

    /// <summary>
    /// Добавляет одну линию к уже существующим
    /// </summary>
    public void AddSegment(Vector2 start, Vector2 end)
    {
        segments.Add(new LineSegment(start, end));
        SetAllDirty();
    }

    public void ClearSegments()
    {
        segments.Clear();
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (segments == null || segments.Count == 0) return;

        // Итерируемся по каждому независимому отрезку
        for (int i = 0; i < segments.Count; i++)
        {
            CreateLineSegment(segments[i].start, segments[i].end, vh);
        }
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, VertexHelper vh)
    {
        Vector2 dir = (end - start).normalized;
        if (dir == Vector2.zero) return;

        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2f);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // 0: Лево-Низ
        vertex.position = start - normal;
        vh.AddVert(vertex);

        // 1: Лево-Верх
        vertex.position = start + normal;
        vh.AddVert(vertex);

        // 2: Право-Верх
        vertex.position = end + normal;
        vh.AddVert(vertex);

        // 3: Право-Низ
        vertex.position = end - normal;
        vh.AddVert(vertex);

        int index = vh.currentVertCount - 4;
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
    private void OnDrawGizmosSelected()
    {
        if (segments == null || segments.Count == 0) return;

        Gizmos.color = Color.green;
        Matrix4x4 localMatrix = transform.localToWorldMatrix;

        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 wStart = localMatrix.MultiplyPoint3x4(segments[i].start);
            Vector3 wEnd = localMatrix.MultiplyPoint3x4(segments[i].end);

            Gizmos.DrawWireSphere(wStart, 8f);
            Gizmos.DrawWireSphere(wEnd, 8f);
            Gizmos.DrawLine(wStart, wEnd);
        }
    }
}