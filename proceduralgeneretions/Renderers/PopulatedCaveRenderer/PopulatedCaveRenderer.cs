using Godot;
using System.Collections.Generic;
[GlobalClass]
public partial class PopulatedCaveRenderer : Renderer<PopulatedCave>
{
    [Export] public PopulatedCaveGenerator Generator;
    [Export] Color CaveColor;
    [Export] public Color[] groupsColors;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate();
    }

    public override void DrawObject()
    {
        foreach (Vector2I cell in GeneratedObject.Cave.PathTiles)
        {
            DrawRect(new Rect2(cell.X * DrawScale, cell.Y * DrawScale, DrawScale, DrawScale), CaveColor);
        }

        int colorIndex = 0;
        foreach (List<Rect2I> group in Generator.groups)
        {
            if (groupsColors != null && groupsColors.Length > 0)
            {
                foreach (Rect2I space in group)
                {
                    DrawRect(new Rect2(
                                    space.Position.X * DrawScale,
                                    space.Position.Y * DrawScale,
                                    space.Size.X * DrawScale,
                                    space.Size.Y * DrawScale),
                            groupsColors[colorIndex]);
                }
                colorIndex = (colorIndex + 1) % groupsColors.Length;
            }
        }

        foreach (Room room in GeneratedObject.Rooms)
        {
            DrawRect(
                new Rect2(
                    room.GetGlobalPosition().X * DrawScale,
                    room.GetGlobalPosition().Y * DrawScale,
                    room.GetSizeInCells().X * DrawScale,
                    room.GetSizeInCells().Y * DrawScale
                ),
                room.Color,
                false,
                1f);
        }
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }
}
