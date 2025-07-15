using Godot;
using Godot.Collections;
using System;
/// <summary>
/// Represents a corridor with a specific side and area.
/// </summary>
public class Corridor : Room
{
    /// <summary>
    /// The side of the corridor.
    /// </summary>
    public Connection.Sides Side;
    public Rect2I Area;

    public Corridor(Connection.Sides Side, Rect2I Area) : base(Area.Position, Area.Size, Colors.Black, null)
    {
        this.Side = Side;
        this.Area = Area;
    }

    /// <summary>
    /// Returns the direction of the corridor's side.
    /// 0 represents horizontal (Starting from Left/Right), 1 represents vertical (Starting from Top/Bottom).
    /// </summary>
    /// <returns>0 for horizontal <see cref="Corridor"/>, 1 for vertical <see cref="Corridor"/>.</returns>
    public int GetDirection()
    {
        return (Side == Connection.Sides.Left || Side == Connection.Sides.Right) ? 0 : 1;
    }
}