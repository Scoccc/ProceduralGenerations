using System;
using Godot;

/// <summary>
/// Represents a connection with a specific side, height, and status.
/// </summary>
[GlobalClass]
public partial class Connection: Resource
{
    /// <summary>
    /// Enum representing the possible sides of a connection.
    /// </summary>
    public enum Sides
    {
        Right,
        Left,
        Top,
        Bottom
    }

    /// <summary>
    /// Enum representing the possible statuses of a connection.
    /// </summary>
    public enum Status
    {
        Free,
        Blocked,
        Connected
    }

    /// <summary>
    /// Gets the height of the connection. If the connection is blocked, height is -1.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The side of the connection.
    /// </summary>
    public Sides Side;

    /// <summary>
    /// The status of the connection.
    /// </summary>
    public Status status;

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class with a specified side and height.
    /// Default status is Free.
    /// </summary>
    /// <param name="side">The side of the connection.</param>
    /// <param name="height">The height of the connection.</param>
    public Connection(Sides side, int height)
    {
        Side = side;
        Height = height;
        status = Status.Free;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class with a specified side, height, and status.
    /// If the status is Blocked, height is set to -1.
    /// </summary>
    /// <param name="side">The side of the connection.</param>
    /// <param name="height">The height of the connection.</param>
    /// <param name="status">The status of the connection.</param>
    public Connection(Sides side, int height, Status status)
    {
        Side = side;
        Height = status == Status.Blocked ? -1 : height;
        this.status = status;
    }

    /// <summary>
    /// Returns a random side from the four possible sides.
    /// </summary>
    /// <returns>A randomly chosen <see cref="Sides"/> value.</returns>
    public static Sides GetRandomSide()
    {
        Random Rng = new Random();
        return (Sides)(Enum.GetValues(typeof(Sides))).GetValue(Rng.Next(4));
    }

    /// <summary>
    /// Returns the direction of the connection's side.
    /// 0 represents horizontal (Top/Bottom), 1 represents vertical (Left/Right).
    /// </summary>
    /// <returns>0 for horizontal sides, 1 for vertical sides.</returns>
    public int GetDirection()
    {
        return Side == Sides.Top || Side == Sides.Bottom ? 0 : 1;
    }
}
