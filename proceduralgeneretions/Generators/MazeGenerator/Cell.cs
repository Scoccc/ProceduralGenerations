using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The Cell class represents a single cell in the maze grid. 
/// It tracks its GlobalPosition, visitation state, neighbors, and connection status.
/// </summary>
public class Cell : GeneratedObject
{
    /// <summary>
    /// The GlobalPosition of this cell in the grid.
    /// </summary>
    private Vector2 GlobalPosition;

    /// <summary>
    /// Indicates whether the cell has been visited during maze generation.
    /// </summary>
    public bool visited { get; private set; }

    /// <summary>
    /// Flag to mark if this cell is a dead-end.
    /// </summary>
    public bool isADeadEnd { get; set; }

    /// <summary>
    /// Indicates if this cell is the starting cell of the maze.
    /// </summary>
    public bool isStart { get; set; }

    /// <summary>
    /// Indicates if this cell is marked as the exit of the maze.
    /// </summary>
    public bool isExit { get; set; }

    /// <summary>
    /// Distance from the start cell used during maze generation.
    /// </summary>
    public int dinstance = 0;

    /// <summary>
    /// A dictionary of neighboring GetPathTiles along with their connection status.
    /// Each key is a Side enum representing a direction.
    /// </summary>
    public Dictionary<Side, (Cell cell, bool isConnected)> neighbours { get; private set; }

    /// <summary>
    /// Constructor that initializes a cell at a given GlobalPosition.
    /// Throws an exception if the GlobalPosition has negative coordinates.
    /// </summary>
    /// <param name="position">The GlobalPosition of the cell in the grid.</param>
    public Cell(Vector2 position)
    {
        if (position.X < 0 || position.Y < 0)
            throw new ArgumentException("GlobalPosition");
        visited = false;
        isADeadEnd = false;
        isStart = false;
        GlobalPosition = position;
        neighbours = new Dictionary<Side, (Cell cell, bool isConnected)>();
        // Initialize neighbors for each side as null and not connected.
        neighbours.Add(Side.Right, (null, false));
        neighbours.Add(Side.Bottom, (null, false));
        neighbours.Add(Side.Left, (null, false));
        neighbours.Add(Side.Top, (null, false));
    }

    /// <summary>
    /// Connects this cell to its neighbor on the given side.
    /// Also sets the reciprocal connection on the neighbor cell.
    /// </summary>
    /// <param name="side">The side of this cell to connect.</param>
    public void ConnectTo(Side side)
    {
        if (neighbours[side].cell == null)
            throw new ArgumentException("side");

        // Set the connection to true for this cell and its neighbor.
        neighbours[side] = (neighbours[side].cell, true);
        neighbours[side].cell.neighbours[side.Opposite()] = (this, true);
    }

    /// <summary>
    /// Adds a neighbor cell for a specified side and optionally sets it as connected.
    /// Also establishes the reciprocal relationship on the neighbor cell.
    /// </summary>
    /// <param name="side">The side where the neighbor is located.</param>
    /// <param name="neighbour">The neighboring cell.</param>
    /// <param name="areConnected">Optional flag to set if they are connected.</param>
    public void AddNeigbour(Side side, Cell neighbour, bool areConnected = false)
    {
        neighbours[side] = (neighbour, areConnected);
        neighbours[side].cell.neighbours[side.Opposite()] = (this, areConnected);
    }

    /// <summary>
    /// Marks the cell as visited and sets its distance from the start.
    /// </summary>
    /// <param name="dinstance">The distance value to assign.</param>
    public void Visit(int dinstance)
    {
        visited = true;
        this.dinstance = dinstance;
    }

    /// <summary>
    /// Retrieves a list of sides that have an unvisited neighbor.
    /// </summary>
    /// <returns>A list of Side enums representing unvisited neighbors.</returns>
    public List<Side> getUnvisitedSides()
    {
        List<Side> unvisitedSides = new List<Side>();

        foreach (Side side in Enum.GetValues(typeof(Side)))
        {
            if (neighbours[side].cell != null && !neighbours[side].cell.visited)
                unvisitedSides.Add(side);
        }

        return unvisitedSides;
    }

    /// <summary>
    /// Gets a list of all connected neighbor GetPathTiles.
    /// </summary>
    /// <returns>A list of connected Cell objects.</returns>
    public List<Cell> getConnectedNeigbours()
    {
        List<Cell> connectedNeigbours = new List<Cell>();

        foreach (var neighbour in neighbours)
        {
            if (neighbour.Value.isConnected)
                connectedNeigbours.Add(neighbour.Value.cell);
        }

        return connectedNeigbours;
    }

    /// <summary>
    /// Returns the neighbor cell on a specified side.
    /// </summary>
    /// <param name="side">The side to retrieve the neighbor from.</param>
    /// <returns>The neighboring Cell.</returns>
    public Cell getNeighbour(Side side)
    {
        return neighbours[side].cell;
    }

    /// <summary>
    /// Determines whether the cell is on the edge of the maze grid.
    /// A cell is considered on the edge if one of its neighbors is null.
    /// </summary>
    /// <returns>True if the cell is at the edge; otherwise, false.</returns>
    public bool isOnTheEdge()
    {
        bool isInASide = false;
        foreach (Side side in Enum.GetValues(typeof(Side)))
        {
            isInASide |= neighbours[side].cell == null;
        }
        return isInASide;
    }

    /// <summary>
    /// Provides a string representation of the cell's GlobalPosition and neighbor connection states.
    /// </summary>
    /// <returns>A formatted string showing the cell details.</returns>
    public override string ToString()
    {
        string str = "";
        str += "GetGlobalPosition: " + GlobalPosition + "\n";
        str += "Right:(" + (neighbours[Side.Right].cell == null ? "null" : "some") + ", " + neighbours[Side.Right].isConnected + ")\n";
        str += "Left:(" + (neighbours[Side.Left].cell == null ? "null" : "some") + ", " + neighbours[Side.Left].isConnected + ")\n";
        str += "Top:(" + (neighbours[Side.Top].cell == null ? "null" : "some") + ", " + neighbours[Side.Top].isConnected + ")\n";
        str += "Bottom:(" + (neighbours[Side.Bottom].cell == null ? "null" : "some") + ", " + neighbours[Side.Bottom].isConnected + ")\n";
        return str;
    }

    public Vector2I GetSize()
    {
        return new Vector2I(1, 1);
    }

    public Vector2 GetLocalPosition()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetGlobalPosition()
    {
        return GlobalPosition;
    }

    public Godot.Collections.Array<Vector2> GetWallsTiles()
    {
        throw new NotImplementedException();
    }

    public Godot.Collections.Array<Vector2> GetPathTiles()
    {
        throw new NotImplementedException();
    }
}
