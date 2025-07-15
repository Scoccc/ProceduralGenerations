using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class PopulatedCave : GeneratedObject
{
    public Cave Cave;
    public List<Room> Rooms;

    public PopulatedCave(Cave cave, List<Room> rooms)
    {
        Cave = cave;
        Rooms = rooms;
    }

    public Vector2 GetGlobalPosition()
    {
        return Cave.GetGlobalPosition();
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
        return Cave.GetSize();
    }

    public Array<Vector2> GetWallsTiles()
    {
        throw new System.NotImplementedException();
    }
}
