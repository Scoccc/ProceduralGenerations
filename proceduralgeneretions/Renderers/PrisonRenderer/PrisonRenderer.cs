using Godot;
using System;
[GlobalClass]
public partial class PrisonRenderer : Renderer<Prison>
{
    [Export] PrisonGenerator Generator;

    [ExportGroup("Groups")]
    [Export] public bool DrawGroupLimits = false;
    [Export(PropertyHint.Range, "1,20, 0.1")] public float GroupWidth = 1.5f;
    [Export(PropertyHint.Range, "0,100")] public Color GroupColor = new Color(0xff000e72);
    [ExportGroup("Rooms")]
    [Export] public Color WallColor = Colors.Black;
    [Export(PropertyHint.Range, "1,20, 0.1")] public float WallWidth = 1.5f;
    [ExportGroup("Corridors")]
    [Export] public Color CorridorColor = Colors.Black;
    [ExportGroup("Doors")]
    [Export] public Color DoorsColor = Colors.Red;
    [Export(PropertyHint.Range, "1,20, 0.1")] public float DoorsWidth = 1.5f;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate();
    }

    public override void DrawObject()
    {
        foreach(Group group in GeneratedObject.Groups)
        {
            if (group != null)
            {
                foreach (Corridor corridor in group.Corridors)
                {
                    Rect2 ScaledRectRoom = new Rect2(corridor.Area.Position.X * DrawScale,
                                                     corridor.Area.Position.Y * DrawScale,
                                                     corridor.Area.Size.X * DrawScale,
                                                     corridor.Area.Size.Y * DrawScale);
                    DrawRect(ScaledRectRoom, CorridorColor, false, WallWidth);
                    DrawRect(ScaledRectRoom, CorridorColor, true);
                }

                foreach (Room room in group.Rooms)
                {
                    Rect2 ScaledRectRoom = new Rect2(room.GetGlobalPosition().X / Room.CellSize * DrawScale,
                                             room.GetGlobalPosition().Y / Room.CellSize * DrawScale,
                                             room.GetSizeInCells().X * DrawScale,
                                             room.GetSizeInCells().Y * DrawScale);
                    DrawRect(ScaledRectRoom, room.Color);
                    DrawRect(ScaledRectRoom, WallColor, false, WallWidth);

                    foreach (var side in room.FreeSides.Keys)
                    {
                        if (room.FreeSides[side])
                        {
                            switch (side)
                            {
                                case Side.Left:
                                    DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                                    break;
                                case Side.Right:
                                    DrawLine(new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y  + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                                    break;
                                case Side.Top:
                                    DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y), DoorsColor, DoorsWidth);
                                    break;
                                case Side.Bottom:
                                    DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                                    break;
                            }
                        }
                    }
                }

                if (DrawGroupLimits)
                {
                    Rect2 ScaledRectGroup = new Rect2(group.GetGlobalPosition().X * DrawScale,
                                                      group.GetGlobalPosition().Y * DrawScale,
                                                      group.GetSize().X * DrawScale,
                                                      group.GetSize().Y * DrawScale);
                    DrawRect(ScaledRectGroup, GroupColor, false, GroupWidth);
                }
            }
        }
        /*
        foreach(var pos in Generator.possibleRooms)
        {
            Rect2 ScaledRectGroup = new Rect2(pos.Coordinates.X * DrawScale,
                                                     pos.Coordinates.Y * DrawScale,
                                                     7 * DrawScale,
                                                     7 * DrawScale);
            DrawRect(ScaledRectGroup, Colors.Green, false, GroupWidth);
        }
        
        foreach (Room room in GeneratedObject.ExternalRooms)
        {
            Rect2 ScaledRectRoom = new Rect2(room.GetGlobalPosition().X / Room.CellSize * DrawScale,
                                     room.GetGlobalPosition().Y / Room.CellSize * DrawScale,
                                     room.GetSizeInCells().X * DrawScale,
                                     room.GetSizeInCells().Y * DrawScale);
            DrawRect(ScaledRectRoom, room.Color);
            DrawRect(ScaledRectRoom, WallColor, false, WallWidth);

            foreach (var side in room.FreeSides.Keys)
            {
                if (room.FreeSides[side])
                {
                    switch (side)
                    {
                        case Side.Left:
                            DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                            break;
                        case Side.Right:
                            DrawLine(new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                            break;
                        case Side.Top:
                            DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y), DoorsColor, DoorsWidth);
                            break;
                        case Side.Bottom:
                            DrawLine(new Vector2(ScaledRectRoom.Position.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), new Vector2(ScaledRectRoom.Position.X + ScaledRectRoom.Size.X, ScaledRectRoom.Position.Y + ScaledRectRoom.Size.Y), DoorsColor, DoorsWidth);
                            break;
                    }
                }
            }
        }*/
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }
}
