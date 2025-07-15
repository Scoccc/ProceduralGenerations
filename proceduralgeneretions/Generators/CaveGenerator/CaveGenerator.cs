 using Godot;
using System.Collections.Generic;
[GlobalClass]
public partial class CaveGenerator : Generator<Caves>
{
    [ExportGroup("Generation")]
    [Export] public Vector2I gridPosition = new Vector2I(0, 0);
    [Export] public Vector2I gridSize = new Vector2I(100, 100);
    [Export] public int borderWidth = 5;
    [ExportSubgroup("Cellular Automaton Parameters")]
    [Export(PropertyHint.Range, "0,8,")] public int maxEmptyNeighbors = 4;
    [Export(PropertyHint.Range, "0,8,")] public int minEmptyNeighbors = 2;
    [Export(PropertyHint.Range, "0,20,1,or_greater")] public int iterations = 10;
    [ExportSubgroup("Noise")]
    [Export(PropertyHint.Range, "0,1,0.001")] public float noiseDensity = 0.598f;
    [ExportSubgroup("Number of Caves")]
    [Export(PropertyHint.Range, "0,1,0.0001")] public float minThreshold = 1f;
    [Export] public ulong seed = 0;

    private Caves caves;
    private Godot.Collections.Array<Vector2I> wallsCells = new Godot.Collections.Array<Vector2I>();
    private bool[,] grid;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    public int biggestCave = 0;
    private Godot.Collections.Array<Vector2I> Walls = new Godot.Collections.Array<Vector2I>();

    /// <summary>
    /// Generates the cave by executing the procedural generation steps.
    /// </summary>

    public override Caves generate()
    {
        grid = new bool[gridSize.X + borderWidth * 2, gridSize.Y + borderWidth * 2];
        rng.Seed = seed;
        caves = new Caves(gridPosition, gridSize);

        Walls.Clear();
        wallsCells.Clear();
        biggestCave = 0;

        SetNoise();
        Smooth();
        MarkCaves();
        ApplyThreshold();

        foreach(Cave cave in caves)
        {
            foreach(Vector2I cell in cave.PathTiles)
            {
                Walls.Remove(cell);
            }
        }
        caves.Walls = Walls;
        return caves;
    }


    /// <summary>
    /// Initializes the grid with random values based on noise density.
    /// </summary>
    private void SetNoise()
    {
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                if(x >= borderWidth && x < gridSize.X - borderWidth &&
                   y >= borderWidth && y < gridSize.Y - borderWidth)
                    grid[x, y] = rng.Randf() < noiseDensity;
                Walls.Add(new Vector2I(x, y));
            }
        }
    }

    /// <summary>
    /// Applies multiple iterations of the cellular automaton to refine the cave structure.
    /// </summary>
    private void Smooth()
    {
        for (int i = 0; i < iterations; i++)
        {
            GD.Print("Iteratore");
            bool[,] _temp_grid = grid;

            for (int x = 0; x < gridSize.X; x++)
            {
                for (int y = 0; y < gridSize.Y; y++)
                {
                    int count = CountDeadNeighbors(x, y);
                    if (grid[x, y] && count > maxEmptyNeighbors)
                    {
                        _temp_grid[x, y] = false;
                    }
                    else if (!grid[x, y] && count <= minEmptyNeighbors)
                        _temp_grid[x, y] = true;
                }
            }

            grid = _temp_grid;
        }
    }

    /// <summary>
    /// Removes small caves based on the minimum threshold and draws the main path.
    /// </summary>
    private void ApplyThreshold()
    {
        caves.RemoveAll(cave => (float)cave.GetArea() / biggestCave < minThreshold);
    }

    /// <summary>
    /// Counts the number of dead (wall) neighbors surrounding a cell.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>The number of dead neighbors.</returns>
    private int CountDeadNeighbors(int x, int y)
    {
        int counter = 0;
        for (int x_offset = -1; x_offset <= 1; x_offset++)
        {
            for (int y_offset = -1; y_offset <= 1; y_offset++)
            {
                if (!(x_offset == 0 && y_offset == 0))
                {
                    if (x + x_offset < 0 ||
                        x + x_offset >= gridSize.X ||
                        y + y_offset < 0 ||
                        y + y_offset >= gridSize.Y ||
                        !grid[x + x_offset, y + y_offset])
                    {
                        counter++;
                    }
                }
            }
        }
        return counter;
    }

    /// <summary>
    /// Identifies and labels connected cave regions.
    /// </summary>
    private void MarkCaves()
    {
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                if (grid[x, y])
                {
                    markCave(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Marks a cave by finding all connected floor tiles.
    /// </summary>
    /// <param name="x">X coordinate of the starting tile.</param>
    /// <param name="y">Y coordinate of the starting tile.</param>
    private void markCave(int x, int y)
    {
        List<Vector2I> connected = new List<Vector2I>();
        Stack<Vector2I> toVisit = new Stack<Vector2I>();

        toVisit.Push(new Vector2I(x, y));
        while (toVisit.Count > 0)
        {
            Vector2I nextPosition = toVisit.Pop();

            if (nextPosition.X > 0 && nextPosition.X < gridSize.X && nextPosition.Y > 0 && nextPosition.Y < gridSize.Y)
            {
                if (grid[nextPosition.X, nextPosition.Y])
                {
                    connected.Add(nextPosition);
                    grid[nextPosition.X, nextPosition.Y] = false;

                    for (int x_offset = -1; x_offset <= 1; x_offset++)
                    {
                        for (int y_offset = -1; y_offset <= 1; y_offset++)
                        {
                            if (Mathf.Abs(x_offset) != Mathf.Abs(y_offset))
                            {
                                toVisit.Push(new Vector2I(nextPosition.X + x_offset, nextPosition.Y + y_offset));
                            }
                        }
                    }
                }
            }
        }

        if (connected.Count > biggestCave) biggestCave = connected.Count;

        caves.Add(new Cave(connected));
    }
}
