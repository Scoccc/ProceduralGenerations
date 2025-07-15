using System;
using Godot;
using Godot.Collections;

public abstract partial class Generator<G> : Node where G : GeneratedObject
{
    public abstract G generate();

    public class InvalidParam : Exception
    {
        public InvalidParam(string message)
            : base(message)
        { }
    }
}

public interface GeneratedObject
{
    public abstract Vector2 GetLocalPosition();
    public abstract Vector2 GetGlobalPosition();
    public abstract Vector2I GetSize();
    public abstract Array<Vector2> GetWallsTiles();
    public abstract Array<Vector2> GetPathTiles();
}



