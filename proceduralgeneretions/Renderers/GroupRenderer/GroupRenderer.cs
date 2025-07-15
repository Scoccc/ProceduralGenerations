using Godot;
using System;

[GlobalClass]
public partial class GroupRenderer : Renderer<Group>
{
    [Export] GroupGenerator Generator;

    [ExportGroup("Groups")]
    [Export] public Vector2I GroupPosition;
    [Export] public bool DrawGroupLimits = false;
    [Export(PropertyHint.Range, "1,20, 0.1")] public float GroupWidth = 1.5f;
    [Export(PropertyHint.Range, "0,100")] public Color GroupColor = new Color(0xff000e72);
    [ExportGroup("Rooms")]
    [Export]public Color WallColor = Colors.Black;
    [Export(PropertyHint.Range, "1,20, 0.1")]public float WallWidth = 1.5f;
    [ExportGroup("Corridors")]
    [Export(PropertyHint.Range, "1,20, 0.1")]public Color CorridorColor = Colors.Black;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate(GroupPosition);
    }
    public override void DrawObject()
    {
        foreach (Corridor corridor in GeneratedObject.Corridors)
        {
            Rect2 ScaledRectRoom = new Rect2(corridor.Area.Position.X * DrawScale,
                                             corridor.Area.Position.Y * DrawScale, 
                                             corridor.Area.Size.X * DrawScale,
                                             corridor.Area.Size.Y * DrawScale);
            DrawRect(ScaledRectRoom, CorridorColor, false, WallWidth);
            DrawRect(ScaledRectRoom, CorridorColor, true);
        }

        foreach (Room room in GeneratedObject.Rooms)
        {
            Rect2 ScaledRectRoom = new Rect2(room.GetGlobalPosition().X / Room.CellSize * DrawScale,
                                             room.GetGlobalPosition().Y / Room.CellSize * DrawScale,
                                             room.GetSizeInCells().X * DrawScale,
                                             room.GetSizeInCells().Y * DrawScale);
            DrawRect(ScaledRectRoom, room.Color);
            DrawRect(ScaledRectRoom, WallColor, false, WallWidth);
        }

        if (DrawGroupLimits)
        {
            Rect2 ScaledRectGroup = new Rect2(GeneratedObject.GetGlobalPosition().X * DrawScale,
                                              GeneratedObject.GetGlobalPosition().Y * DrawScale,
                                              GeneratedObject.GetSize().X * DrawScale,
                                              GeneratedObject.GetSize().Y * DrawScale);
            DrawRect(ScaledRectGroup, GroupColor, false, GroupWidth);
        }
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }
}
