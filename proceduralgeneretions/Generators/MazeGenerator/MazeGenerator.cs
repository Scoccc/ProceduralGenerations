using Godot;
using System;
using System.Collections.Generic;
[GlobalClass]
public partial class MazeGenerator : Generator<Maze>
{
    /// <summary>
    /// Enum to choose the method for selecting the starting cell.
    /// </summary>
    public enum ChooseStartMethod
    {
        Fixed,  // A fixed starting GlobalPosition.
        Random  // A random starting GlobalPosition.
    }

    /// <summary>
    /// Enum to choose the method for selecting the exit cell.
    /// </summary>
    public enum ChooseExitMethod
    {
        Further,  // The exit is the farthest reached cell.
        Fixed,     // The exit is a fixed GlobalPosition.
        Random     // The exit is randomly chosen.
    }
    // Generation settings
    [ExportGroup("Generation")]
    [ExportSubgroup("GetSize")]
    [Export] public Vector2I Position = new Vector2I(0, 0);
    [Export] public Vector2I MazeSize = new Vector2I(20, 20);
    [ExportSubgroup("Start")]
    [Export] public ChooseStartMethod StartChoosingMethod { get; set; }
    [Export] public bool ChooseOuterStart { get; set; } = true;
    [Export] public Vector2I StartPosition;
    [ExportSubgroup("Exit")]
    [Export] public ChooseExitMethod ExitChoosingMethod { get; set; }
    [Export] public bool ChooseOuterExits { get; set; } = true;
    [Export] public Vector2I ExitPosition;

    private Stack<Cell> stack = new Stack<Cell>();
    private Maze maze;
    private Cell initialCell;
    public Cell FurtherExit;
    public Cell FurtherOuterExit;
    private List<Cell> possibleExits = new List<Cell>();
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private float connectionWidth;

    public override Maze generate()
    {
        stack.Clear();
        possibleExits.Clear();

        CreateCells();
        if (StartChoosingMethod == ChooseStartMethod.Fixed)
            CreateConnections(StartPosition);
        else{
            if (ChooseOuterStart){
                // Randomize between choosing an X coordinate (top or bottom) or Y coordinate (left or right)
                if (rng.RandiRange(0, 1) == 0) { // Randomize X
                    if (rng.RandiRange(0, 1) == 0){  // Left side
                        CreateConnections(new Vector2I(rng.RandiRange(0, MazeSize.X - 1), 0));
                    }
                    else {  // Right Side
                        CreateConnections(new Vector2I(rng.RandiRange(0, MazeSize.X - 1), MazeSize.Y - 1));
                    }
                }
                else{ // Randomize Y
                    if (rng.RandiRange(0, 1) == 0) { // Top Side
                        CreateConnections(new Vector2I(0, rng.RandiRange(0, MazeSize.Y - 1)));
                    }
                    else{ // Bottom Side
                        CreateConnections(new Vector2I(MazeSize.X - 1, rng.RandiRange(0, MazeSize.Y - 1)));
                    }
                }
            }
            else{ // Randomize both X and Y if not limited to outer GetPathTiles.
                CreateConnections(new Vector2I(rng.RandiRange(0, MazeSize.X), rng.RandiRange(0, MazeSize.Y)));
            }
        }
        // Determine the exit cell based on the chosen exit method.
        switch (ExitChoosingMethod)
        {
            case ChooseExitMethod.Further:
                if (ChooseOuterExits)
                {
                    if (possibleExits.Exists(c => c.isOnTheEdge()))
                    {
                        List<Cell> list = possibleExits.FindAll(c => c.isOnTheEdge());
                        list.Sort((a, b) => b.dinstance - a.dinstance);
                        list[0].isExit = true;
                    }
                    else
                    {
                        possibleExits.GetRandomElement().isExit = true;
                    }
                }
                else
                {
                    FurtherExit.isExit = true;
                }
                break;

            case ChooseExitMethod.Fixed:
                // Validate exit GlobalPosition and ensure it does not coincide with the start GlobalPosition.
                if (ExitPosition.X < 0 || ExitPosition.X >= MazeSize.X)
                    throw new ArgumentException("ExitPosition.X");
                else if (ExitPosition.Y < 0 || ExitPosition.Y >= MazeSize.Y)
                    throw new ArgumentException("ExitPosition.Y");
                else if (ExitPosition == StartPosition)
                    throw new ArgumentException("ExitPosition and StartPosition are the same");
                maze[ExitPosition.X, ExitPosition.Y].isExit = true;
                break;

            case ChooseExitMethod.Random:
                if (ChooseOuterExits)
                {
                    if (possibleExits.Exists(c => c.isOnTheEdge()))
                        possibleExits.FindAll(c => c.isOnTheEdge()).GetRandomElement().isExit = true;
                    else
                        possibleExits.GetRandomElement().isExit = true;
                }
                else
                {
                    possibleExits.GetRandomElement().isExit = true;
                }
                break;
        }

        return maze;
    }

    /// <summary>
    /// Creates the maze grid by instantiating Cell objects and linking them to their neighbors.
    /// </summary>
    private void CreateCells()
    {
        if (MazeSize.X < 2)
            throw new ArgumentException("MazeSize.X");
        if (MazeSize.Y < 2)
            throw new ArgumentException("MazeSize.Y");

        maze = new Maze(Position, MazeSize.X, MazeSize.Y);
        for (int y = 0; y < MazeSize.Y; y++)
        {
            for (int x = 0; x < MazeSize.X; x++)
            {
                Cell cell = new Cell(new Vector2I(x, y));

                // Link the cell with its left neighbor if available.
                if (x > 0)
                    cell.AddNeigbour(Side.Left, maze[x - 1, y]);
                // Link the cell with its top neighbor if available.
                if (y > 0)
                    cell.AddNeigbour(Side.Top, maze[x, y - 1]);

                maze[x, y] = cell;
            }
        }
    }

    /// <summary>
    /// Creates the maze paths starting from a given GlobalPosition using a depth-first search approach.
    /// It marks the start cell, connects GetPathTiles randomly, and tracks dead ends.
    /// </summary>
    /// <param name="StartPosition">The starting cell GlobalPosition.</param>
    private void CreateConnections(Vector2I StartPosition)
    {
        if (StartPosition.X < 0 || StartPosition.X >= MazeSize.X)
            throw new ArgumentException("StartPosition.X");
        else if (StartPosition.Y < 0 || StartPosition.Y >= MazeSize.Y)
            throw new ArgumentException("StartPosition.Y");

        int currentDinstance = 0;
        int longestDinstance = 0;

        // Set the starting cell and mark it as visited.
        initialCell = maze[StartPosition.X, StartPosition.Y];
        initialCell.isStart = true;
        initialCell.Visit(currentDinstance);
        stack.Push(initialCell);

        // Use a stack to perform a depth-first search through the maze.
        while (stack.Count > 0)
        {
            Cell currentCell = stack.Pop();
            List<Side> unvisitedSides = currentCell.getUnvisitedSides();
            if (unvisitedSides.Count > 0)
            {
                // Continue path from the current cell by selecting a random unvisited neighbor.
                stack.Push(currentCell);
                Side choosenSide = unvisitedSides.GetRandomElement();
                Cell choosenNeighbour = currentCell.getNeighbour(choosenSide);
                currentCell.ConnectTo(choosenSide);
                choosenNeighbour.Visit(++currentDinstance);
                // Track the farthest reached cell for potential exit selection.
                if (currentDinstance > longestDinstance)
                {
                    longestDinstance = currentDinstance;
                    FurtherExit = choosenNeighbour;
                }
                stack.Push(choosenNeighbour);
            }
            else
            {
                // Backtrack when there are no unvisited neighbors.
                currentDinstance--;
                currentCell.isADeadEnd = currentCell.getConnectedNeigbours().Count == 1;
                // Add dead-end GetPathTiles (not the start) to the possible exit list.
                if (currentCell.isADeadEnd && !currentCell.isStart)
                {
                    possibleExits.Add(currentCell);
                }
            }
        }
    }
}