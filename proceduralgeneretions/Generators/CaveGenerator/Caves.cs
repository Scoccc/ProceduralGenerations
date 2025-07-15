
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Caves : List<Cave>, GeneratedObject
{
    private Vector2 GlobalPosition;
    private Vector2I Size;
    public Godot.Collections.Array<Vector2I> Walls = new Godot.Collections.Array<Vector2I>();
    public Caves (Vector2I Position, Vector2I Size)
    {
        new List<Cave>();
        GlobalPosition = Position;
        this.Size = Size;
    }

    public Vector2I GetSize()
    {
        return Size;
    }

    public Vector2 GetLocalPosition()
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetGlobalPosition()
    {
        return GlobalPosition;
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
