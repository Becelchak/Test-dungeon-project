using System;
using UnityEngine;
using static DoorConnection;

public class DoorMarker : MonoBehaviour
{
    [SerializeField] private DoorSide side;
    [SerializeField] private float width = 2f; // ширина прохода (опционально)
    [SerializeField] private Vector3 pivotOffset; // смещение относительно центра двери
    [SerializeField] private bool isClosedDoor;
    public bool IsClosedDoor => isClosedDoor;
    public DoorSide Side => side;
    public Vector3 LocalPosition => transform.localPosition + pivotOffset;
    public Vector3 Forward => transform.forward;
    public Quaternion Rotation => transform.rotation;
    public float Width => width;

    public void SetSide(DoorSide newSide) => side = newSide;

    public void SetDoorCloseStatus(bool status) => isClosedDoor = status;

    public DoorMarker(DoorSide side, float width, Vector3 pivotOffset)
    {
        this.side = side;
        this.width = width;
        this.pivotOffset = pivotOffset;
    }

    private void OnDrawGizmos()
    {
        if (IsClosedDoor)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);

        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Vector3 checkPoint = transform.position + transform.forward * 0.5f;
        Gizmos.DrawCube(checkPoint, Vector3.one * 0.2f);
    }
}