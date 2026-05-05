using System;
using UnityEngine;

public class DoorConnection
{
    public RoomInstance roomA;
    public RoomInstance roomB;
    public Transform doorMarkerA;
    public Transform doorMarkerB;
    public DoorSide sideA;
    public DoorSide sideB => (DoorSide)(-(int)sideA);
    public DoorConnection(RoomInstance roomA, RoomInstance roomB, Transform doorMarkerA, Transform doorMarkerB, DoorSide sideA)
    {
        this.roomA = roomA;
        this.roomB = roomB;
        this.doorMarkerA = doorMarkerA;
        this.doorMarkerB = doorMarkerB;
        this.sideA = sideA;
    }

    public static DoorSide OppositeSide(DoorSide side)
    {
        switch (side)
        {
            case DoorSide.North: return DoorSide.South;
            case DoorSide.South: return DoorSide.North;
            case DoorSide.East: return DoorSide.West;
            case DoorSide.West: return DoorSide.East;
            default: throw new ArgumentOutOfRangeException(nameof(side));
        }
    }

    public enum DoorSide
    {
        West = 2,
        North = 1,
        East = -2,
        South = -1,
    }
}