using System.Collections.Generic;

public class FloorPlan
{
    public List<RoomInstance> rooms;
    public int startRoomIndex = 0;
    public int exitRoomIndex;
    public List<DoorConnection> connections;

    public FloorPlan(List<RoomInstance> rooms)
    {
        this.rooms = rooms;
        this.connections = new List<DoorConnection>();
        exitRoomIndex = rooms.Count - 1;
    }
}