using Godot;
using System;

public abstract partial class Renderer<G> : Node2D where G : GeneratedObject
{
    [Export(PropertyHint.Range, "0,20,0.1,or_greater")] public float DrawScale = 5f;
    [ExportGroup("Grid")]
    [Export] public bool ToDrawGrid = false;
    [Export(PropertyHint.Range, "1,20, 0.1")] public float GridWidth = 1.0f;
    [Export] public Color GridColor = new Color(0x4c4c4c30);
    [ExportGroup("Tiles")]
    [Export] public bool drawTiles = false;
    [ExportSubgroup("Walls")]
    [Export] public TileMapLayer wallsTileMapLayer;
    [Export] public int terrainSet = 0;
    [Export] public int terrain = 0;
    [ExportSubgroup("Path")]
    [Export] public TileMapLayer pathTileMapLayer;
    [Export] public int sourceId;
    [Export] public Vector2I atlasCord;

    public G GeneratedObject;

    public void RenderDraw()
    {
        if(drawTiles)
        {
            DrawTiles();
        }
        else
        {
            
        }
    }

    public void DrawGrid()
    {
        for (int i = 0; i < GeneratedObject.GetSize().X; i++)
        {
            DrawLine(new Vector2(i * DrawScale, 0), new Vector2(i * DrawScale, GeneratedObject.GetSize().Y * DrawScale), GridColor, GridWidth);
        }

        for (int i = 0; i < GeneratedObject.GetSize().Y; i++)
        {
            DrawLine(new Vector2(0, i * DrawScale), new Vector2(GeneratedObject.GetSize().X * DrawScale, i * DrawScale), GridColor, GridWidth);
        }
    }

    public abstract void DrawObject();

    public abstract void DrawTiles();

    public override void _Draw()
    {
        if(GeneratedObject != null)
        {
            if(drawTiles)
                DrawTiles();
            else
            {
                DrawObject();
                if (ToDrawGrid)
                    DrawGrid();
            }
        }
    }
}

