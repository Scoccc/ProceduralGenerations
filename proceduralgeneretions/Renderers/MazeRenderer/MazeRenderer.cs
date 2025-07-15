using Godot;
using System;

[GlobalClass]
public partial class MazeRenderer : Renderer<Maze>
{
    [Export] MazeGenerator Generator;
    [Export] public bool ShowDijkstraMap = false;
    [Export] public Color ClosestColor = Colors.Red;
    [Export] public Color FurtherColor = Colors.Blue;
    [Export] public Color PathColor = Colors.White;
    [Export] public Color WallsColor = Colors.Black;
    [Export] public Color StartingPointColor = Colors.Green;
    [Export] public bool ShowDeadEnds = true;
    [Export] public Color DeadEndsColor = Colors.Orange;
    [Export] public Color ExitColor = Colors.Red;

    public override void _Ready()
    {
        GeneratedObject = Generator.generate();
    }

    public override void DrawObject()
    {
        float connectionWidth = DrawScale / 2.0f;

        // Draw the overall maze background (walls).
        DrawRect(new Rect2(0, 0, DrawScale * Generator.MazeSize.X, DrawScale * Generator.MazeSize.Y), WallsColor, true);

        // Loop through each cell to draw grid lines and paths.
        for (int y = 0; y < Generator.MazeSize.Y; y++)
        {
            for (int x = 0; x < Generator.MazeSize.X; x++)
            {
                Vector2 cellCenter = new Vector2(x * DrawScale + DrawScale / 2f, y * DrawScale + DrawScale / 2f);

                if (ShowDijkstraMap)
                {
                    PathColor = ClosestColor.Lerp(FurtherColor, (float)GeneratedObject[x, y].dinstance / Generator.FurtherExit.dinstance);
                }

                // Draw connections (paths) if the neighbor in a given direction is connected.
                if (GeneratedObject[x, y].neighbours[Side.Right].isConnected)
                    DrawLine(cellCenter, new Vector2(x * DrawScale + DrawScale, cellCenter.Y), PathColor, connectionWidth);
                if (GeneratedObject[x, y].neighbours[Side.Left].isConnected)
                    DrawLine(cellCenter, new Vector2(x * DrawScale, cellCenter.Y), PathColor, connectionWidth);
                if (GeneratedObject[x, y].neighbours[Side.Top].isConnected)
                    DrawLine(cellCenter, new Vector2(cellCenter.X, y * DrawScale), PathColor, connectionWidth);
                if (GeneratedObject[x, y].neighbours[Side.Bottom].isConnected)
                    DrawLine(cellCenter, new Vector2(cellCenter.X, y * DrawScale + DrawScale), PathColor, connectionWidth);

                // Draw indicators for the start, exit, dead ends, or regular path GetPathTiles.
                if (GeneratedObject[x, y].isStart)
                    DrawRect(new Rect2((x * DrawScale + connectionWidth / 2), (y * DrawScale + connectionWidth / 2), connectionWidth, connectionWidth), StartingPointColor, true);
                else if (GeneratedObject[x, y].isExit)
                    DrawRect(new Rect2((x * DrawScale + connectionWidth / 2), (y * DrawScale + connectionWidth / 2), connectionWidth, connectionWidth), ExitColor, true);
                else if (GeneratedObject[x, y].isADeadEnd && ShowDeadEnds)
                    DrawRect(new Rect2((x * DrawScale + connectionWidth / 2), (y * DrawScale + connectionWidth / 2), connectionWidth, connectionWidth), DeadEndsColor, true);
                else
                    DrawRect(new Rect2((x * DrawScale + connectionWidth / 2), (y * DrawScale + connectionWidth / 2), connectionWidth, connectionWidth), PathColor, true);
            }
        }
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }
}
