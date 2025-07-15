using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a group that contains rooms, corridors, and connections within a specific rectangular area.
/// This class extends Node and provides methods to add rooms and corridors to the group.
/// </summary>
public class Group : GeneratedObject
{
    /// <summary>
    /// The rectangular area covered by the group.
    /// </summary>
    private Rect2I Area;

    /// <summary>
    /// The collection of rooms that belong to the group.
    /// </summary>
    public List<Room> Rooms { get; }

    /// <summary>
    /// The collection of corridors that belong to the group.
    /// </summary>
    public List<Corridor> Corridors { get; }

    /// <summary>
    /// The collection of connections that belong to the group.
    /// </summary>
    public List<Connection> Connections { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Group"/> class with a specified area.
    /// </summary>
    /// <param name="Area">The rectangular area for the group.</param>
    public Group(Rect2I Area)
    {
        this.Area = Area;
        Rooms = new List<Room>();
        Connections = new List<Connection>();
        Corridors = new List<Corridor>();
    }

    /// <summary>
    /// Adds a room to the group's collection of rooms.
    /// </summary>
    /// <param name="Room">The room to add.</param>
    public void AddRoom(Room Room)
    {
        Rooms.Add(Room);
    }

    /// <summary>
    /// Adds a corridor to the group's collection of corridors.
    /// </summary>
    /// <param name="Corridor">The corridor to add.</param>
    public void AddCorridor(Corridor Corridor)
    {
        Corridors.Add(Corridor);
    }

    public Vector2 GetLocalPosition()
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetGlobalPosition()
    {
        return Area.Position;
    }

    public Vector2I GetSize()
    {
        return Area.Size;
    }

    public Array<Vector2> GetWallsTiles()
    {
        throw new System.NotImplementedException();
    }

    public Array<Vector2> GetPathTiles()
    {
        throw new System.NotImplementedException();
    }
}
