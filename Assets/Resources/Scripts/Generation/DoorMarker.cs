using System;
using UnityEngine;
using static DoorConnection;

public class DoorMarker : MonoBehaviour
{
    [SerializeField] private DoorSide side;
    [SerializeField] private float width = 2f; // ширина прохода (опционально)
    [SerializeField] private Vector3 pivotOffset; // смещение относительно центра двери

    public DoorSide Side => side;
    public Vector3 LocalPosition => transform.localPosition + pivotOffset;
    public Vector3 Forward => transform.forward;
    public Quaternion Rotation => transform.rotation;
    public float Width => width;

    public void SetSide(DoorSide newSide) => side = newSide;

    public DoorMarker(DoorSide side, float width, Vector3 pivotOffset)
    {
        this.side = side;
        this.width = width;
        this.pivotOffset = pivotOffset;
    }
}