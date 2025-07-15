using Godot;
using Godot.Collections;
using System;

public partial class Maze : GeneratedObject
{
    private readonly Cell[,] _cells;
    private Vector2 GlobalPosition; 
    public int Width => _cells.GetLength(0);
    public int Height => _cells.GetLength(1);

    public Maze(Vector2I Position, int width, int height)
    {
        _cells = new Cell[width, height];
        GlobalPosition = Position;
    }

    public Cell this[int x, int y]
    {
        get => _cells[x, y];
        set => _cells[x, y] = value;
    }

    public Cell[,] Cells => _cells;

    public Vector2I GetSize()
    {
        return new Vector2I(Width, Height);
    }

    public Vector2 GetLocalPosition()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetGlobalPosition()
    {
        return GlobalPosition;
    }

    public Array<Vector2> GetWallsTiles()
    {
        throw new NotImplementedException();
    }

    public Array<Vector2> GetPathTiles()
    {
        throw new NotImplementedException();
    }
}
