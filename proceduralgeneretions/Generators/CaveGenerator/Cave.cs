using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Cave: GeneratedObject
{
    public List<Vector2I> PathTiles { get; private set; }
    private Vector2 GlobalPosition;
    private Vector2I Size;
    public Cave(List<Vector2I> cells)
    {
        int Width;
        int Height;
        int left_end = int.MaxValue;
        int right_end = int.MinValue;
        int top_end = int.MaxValue;
        int bottom_end = int.MinValue;

        this.PathTiles = cells;

        foreach (Vector2I cord in cells)
        {
            left_end = Mathf.Min(left_end, cord.X);
            right_end = Mathf.Max(right_end, cord.X);
            top_end = Mathf.Min(top_end, cord.Y);
            bottom_end = Mathf.Max(bottom_end, cord.Y);
        }

        Width = right_end - left_end + 1;
        Height = bottom_end - top_end + 1;

        GlobalPosition = new Vector2I(left_end, top_end);
        Size = new Vector2I(Width, Height);
    }
    /// <summary>
    /// Returns the number of GetPathTiles of the cave
    /// </summary>
    /// <returns></returns>
    public int GetArea() { return PathTiles.Count; }

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
