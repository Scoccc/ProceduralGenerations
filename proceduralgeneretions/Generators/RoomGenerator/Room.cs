using Godot;
using System;
using Godot.Collections;

public class Room : GeneratedObject
{
    public static int CellSize = 7;
    public Vector2 GlobalPosition { get; set; }
    protected Vector2I SizeInCells { get; set; }
    protected Vector2I SizeInTiles { get; set; }
    public Rect2I TilesArea { get; set; }
    public Color Color { get; set; }

    public byte[] bitMasks;

    public Dictionary<Side, bool> FreeSides;

    public Array<Vector2> Walls;

    public Room(Vector2 Position, Vector2I SizeInCells, Color Color, Dictionary<Side, bool> FreeSides)
    {
        this.GlobalPosition = Position;
        this.SizeInCells = SizeInCells;
        SizeInTiles = SizeInCells * CellSize;
        this.Color = Color;
        this.FreeSides = FreeSides;
        bitMasks = CreateBitMasks();
    }

    public Rect2I GetArea()
    {
        return new Rect2I(new Vector2I((int)GlobalPosition.X, (int)GlobalPosition.Y), SizeInTiles);
    }

    public Vector2 GetGlobalPosition()
    {
        return GlobalPosition;
    }

    public Vector2 GetLocalPosition()
    {
        throw new NotImplementedException();
    }

    public Vector2I GetSize()
    {
        return SizeInTiles;
    }

    public Vector2I GetSizeInCells()
    {
        return SizeInCells;
    }

    public Array<Vector2> GetWallsTiles()
    {
        throw new NotImplementedException();
    }

    public Array<Vector2> GetPathTiles()
    {
        throw new NotImplementedException();
    }

    private byte[] CreateBitMasks()
    {
        int size = SizeInCells.X * SizeInCells.Y;
        byte[] masks = new byte[size];

        for(int i = 0; i < size; i++)
        {
            byte mask = 255;
            bool top = i < SizeInCells.X;
            bool bottom = i >= size - SizeInCells.X;
            bool left = i % SizeInCells.X == 0;
            bool right = (i+1) % SizeInCells.X == 0;

            if (top) mask &= 0b00111110;
            if (bottom) mask &= 0b11100011;
            if (left) mask &= 0b11111000;
            if (right) mask &= 0b10001111;

            masks[i] = mask;
        }

        return masks;
    }

    
}
