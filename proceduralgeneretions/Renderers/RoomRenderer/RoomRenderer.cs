using Godot;
using System;
[GlobalClass]
public partial class RoomRenderer : Renderer<Room>
{
    [Export] public RoomGenerator Generator;
    [Export] public Color WallsColor = Colors.Black;
    [Export] public float WallsWidth = 1.5f;
    [Export] public Color SpaceLimitColor = new Color(0xff000040);
    [Export] public float SpaceLimitWidth = 1.0f;
    [Export] public bool DrawOnlyOutline = false;
    [Export] public bool DrawSpaceLimit = true;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate();
    }
    public override void DrawObject()
    {
        if(DrawSpaceLimit)
        {
            DrawRect(
                new Rect2(
                    Generator.MaxFreeSpace.Position.X * DrawScale,
                    Generator.MaxFreeSpace.Position.Y * DrawScale,
                    Generator.MaxFreeSpace.Size.X * DrawScale,
                    Generator.MaxFreeSpace.Size.Y * DrawScale
                ),
                SpaceLimitColor,
                false,
                SpaceLimitWidth
            );
        }

        if(DrawOnlyOutline)
        {
            DrawRect(
            new Rect2(
                GeneratedObject.GetGlobalPosition().X * DrawScale,
                GeneratedObject.GetGlobalPosition().Y * DrawScale,
                GeneratedObject.GetSize().X * DrawScale,
                GeneratedObject.GetSize().Y * DrawScale
            ),
            GeneratedObject.Color,
            false,
            WallsWidth);
        }
        else
        {
            DrawRect(
                new Rect2(
                    GeneratedObject.GetGlobalPosition().X * DrawScale,
                    GeneratedObject.GetGlobalPosition().Y * DrawScale,
                    GeneratedObject.GetSize().X * DrawScale,
                    GeneratedObject.GetSize().Y * DrawScale
                ),
                GeneratedObject.Color
            );

            DrawRect(
                new Rect2(
                    GeneratedObject.GetGlobalPosition().X * DrawScale,
                    GeneratedObject.GetGlobalPosition().Y * DrawScale,
                    GeneratedObject.GetSize().X * DrawScale,
                    GeneratedObject.GetSize().Y * DrawScale
                ),
                WallsColor,
                false,
                WallsWidth
            );

            foreach(var side in GeneratedObject.FreeSides.Keys)
            {
                if (GeneratedObject.FreeSides[side])
                {
                    switch (side)
                    {
                        case Side.Left:
                            DrawLine(new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale), new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale + GeneratedObject.GetSize().Y * DrawScale), Colors.Red, 2f);
                            break;
                        case Side.Right:
                            DrawLine(new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale + GeneratedObject.GetSize().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale), new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale + GeneratedObject.GetSize().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale + GeneratedObject.GetSize().Y * DrawScale), Colors.Red, 2f);
                            break;
                        case Side.Top:
                            DrawLine(new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale), new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale + GeneratedObject.GetSize().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale), Colors.Red, 2f);
                            break;
                        case Side.Bottom:
                            DrawLine(new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale + GeneratedObject.GetSize().Y * DrawScale), new Vector2(GeneratedObject.GetGlobalPosition().X * DrawScale + GeneratedObject.GetSize().X * DrawScale, GeneratedObject.GetGlobalPosition().Y * DrawScale + GeneratedObject.GetSize().Y * DrawScale), Colors.Red, 2f);
                            break;
                    }
                }
            }
            
        }  
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }
}
