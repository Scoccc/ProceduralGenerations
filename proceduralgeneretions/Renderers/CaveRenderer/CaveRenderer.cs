using Godot;
using System;
[GlobalClass]
public partial class CaveRenderer : Renderer<Caves>
{
    [Export] public CaveGenerator Generator { get; set; }
    [ExportGroup("Drawing")]
    [Export] public Color biggestCaveColor = Colors.Red;
    [Export] public Color smallestCaveColor = Colors.Blue;
    [Export] public Color wallsColor = Colors.Black;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate(); //TODO: cerca di spostarlo nella classe generica
    }

    public override void DrawObject()
    {
        DrawRect(new Rect2(0, 0, Generator.gridSize.X * DrawScale, Generator.gridSize.Y * DrawScale), wallsColor);

        foreach (Cave cave in GeneratedObject)
        {
            foreach (Vector2I cell in cave.PathTiles)
            {
                DrawRect(new Rect2(cell.X * DrawScale, cell.Y * DrawScale, DrawScale, DrawScale), smallestCaveColor.Lerp(biggestCaveColor, cave.GetArea() / (float)Generator.biggestCave));
            }
        }
    }

    public override void DrawTiles()
    {
        for(int x = 0; x < Generator.gridSize.X; x++)
            for(int y = 0; y < Generator.gridSize.Y; y++)
                pathTileMapLayer.SetCell(new Vector2I(x,y), sourceId, atlasCord);

        wallsTileMapLayer.SetCellsTerrainConnect(GeneratedObject.Walls, terrainSet, terrain);
    }
}
