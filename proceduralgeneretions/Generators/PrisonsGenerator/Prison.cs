using Godot;
using Godot.Collections;
using System.Collections.Generic;



public class Prison : GeneratedObject
{
    /// <summary>
    /// The grid of groups that form the layout.
    /// </summary>
    public Group[,] Groups;
    public List<Room> ExternalRooms;
    public int top = int.MaxValue;
    public int bottom = int.MinValue;
    public int left = int.MaxValue;
    public int right = int.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Layout"/> class with the specified dimensions.
    /// </summary>
    /// <param name="Size">The dimensions of the layout grid, where GetSize.X represents the number of columns and GetSize.Y represents the number of rows.</param>
    public Prison(Vector2I Size)
    {
        Groups = new Group[Size.X, Size.Y];
        ExternalRooms = new List<Room>();
    }

    public Vector2 GetGlobalPosition()
    {
        return new Vector2I(left, top);
    }

    public Vector2 GetLocalPosition()
    {
        throw new System.NotImplementedException();
    }

    public Array<Vector2> GetPathTiles()
    {
        throw new System.NotImplementedException();
    }
    
    public Vector2I GetSize()
    {
        return new Vector2I(right - left,bottom - top);
    }

    public Array<Vector2> GetWallsTiles()
    {
        throw new System.NotImplementedException();
    }
}
