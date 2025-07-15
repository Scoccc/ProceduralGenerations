using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
[GlobalClass]
public partial class PrisonGenerator : Generator<Prison>
{
    
    [ExportGroup("Groups")]
    /// <summary>
    /// Il numero usato per la ricorsione.
    /// Non indica il numero esatto di gruppi generati, ma il primo gruppo parte con n, e ad ogni suo figlio passa n-1, e poi si fermano quando raggiungono 0
    /// </summary>
    [Export(PropertyHint.Range, "1,500")] public int RecursionSteps;

    /// <summary>
    /// Il GroupGenerator usato per creare i gruppi
    /// </summary>
    [Export] public GroupGenerator groupGenerator;
    [ExportGroup("External Rooms")]
    /// <summary>
    /// Il generatore di stanze usato per generare le "ExternalRooms", ovvero le stanze piazzate ai confini della mappa (alla fine dei corridoi)
    /// </summary>
    [Export] public RoomGenerator ExternalRoomGenerator;

    private Prison GeneratedPrison;
    private RandomNumberGenerator Rng = new RandomNumberGenerator();

    /// <summary>
    /// Tripla usata per trovare la posizione delle stanze esterne possibili.
    /// Coordinates indica la posizione dell'area
    /// Position insica il lato che si affaccia verso il corridio
    /// height indica l'altezza del corridoio
    /// </summary>
    public List<(Vector2 Coordinates, Side Position, int height)> possibleRooms;

    /// <summary>
    /// La grandezza della griglia che ospita i gruppi
    /// </summary>
    public Vector2I Size = new Vector2I(100, 100);

    private int top = int.MaxValue;
    private int bottom = int.MinValue;
    private int left = int.MaxValue;
    private int right = int.MinValue;

    public override Prison generate()
    {
        generate(RecursionSteps);
        GeneratedPrison.top = top;
        GeneratedPrison.bottom = bottom;
        GeneratedPrison.left = left;
        GeneratedPrison.right = right;

        possibleRooms = new List<(Vector2, Side, int)>();

        FixPatternBetweenGroups();
        PlaceExternalRooms();
        return GeneratedPrison;
    }

    /// <summary>
    /// Starts the layout generation process using the specified recursion steps.
    /// The process begins at the center of the grid.
    /// </summary>
    /// <param name="RecursionSteps">The number of recursive steps to perform during generation.</param>
    public void generate(int RecursionSteps)
    {
        GeneratedPrison = new Prison(Size);
        Vector2I gridPosition = new Vector2I(Size.X / 2, Size.Y / 2); // Center of the matrix.
        Vector2I globalPosition = new Vector2I(0, 0); // Starting GlobalPosition in global coordinates.
        generateRec(gridPosition, globalPosition, RecursionSteps);
    }

    /// <summary>
    /// Recursively generates groups on the layout grid.
    /// For each placed group, free connections are processed and new groups are generated in adjacent positions.
    /// </summary>
    /// <param name="MatrixPosition">The current grid GlobalPosition where a group is to be placed.</param>
    /// <param name="GlobalPosition">The global GlobalPosition corresponding to the grid GlobalPosition.</param>
    /// <param name="GroupsLeft">The number of groups remaining to be generated.</param>
    public void generateRec(Vector2I MatrixPosition, Vector2I GlobalPosition, int GroupsLeft)
    {
        if (GroupsLeft > 0)
        {
            Group PlacedGroup = placeGroup(MatrixPosition, GlobalPosition);
            if (PlacedGroup != null)
            {
                if (PlacedGroup.GetGlobalPosition().X + PlacedGroup.GetSize().X > right) right = (int)PlacedGroup.GetGlobalPosition().X + PlacedGroup.GetSize().X;
                if (PlacedGroup.GetGlobalPosition().X < left) left = (int)PlacedGroup.GetGlobalPosition().X;
                if (PlacedGroup.GetGlobalPosition().Y < top) top = (int)PlacedGroup.GetGlobalPosition().Y;
                if (PlacedGroup.GetGlobalPosition().Y + PlacedGroup.GetSize().Y > bottom) bottom = (int)PlacedGroup.GetGlobalPosition().Y + PlacedGroup.GetSize().Y;
                List<Connection> FreeConnections = PlacedGroup.Connections.FindAll(c => c.status == Connection.Status.Free);
                ShuffleConnectionList(FreeConnections);
                foreach (Connection connection in FreeConnections)
                {
                    connection.status = Connection.Status.Connected;
                    if (GroupsLeft > 0)
                    {
                        GroupsLeft--;
                        Vector2I newMatrixPosition = new Vector2I();
                        Vector2I newGlobalPosition = new Vector2I();

                        switch (connection.Side)
                        {
                            case Connection.Sides.Top:
                                newMatrixPosition = new Vector2I(MatrixPosition.X, MatrixPosition.Y - 1);
                                newGlobalPosition = new Vector2I(GlobalPosition.X, GlobalPosition.Y - groupGenerator.Size.Y);

                                break;

                            case Connection.Sides.Bottom:
                                newMatrixPosition = new Vector2I(MatrixPosition.X, MatrixPosition.Y + 1);
                                newGlobalPosition = new Vector2I(GlobalPosition.X, GlobalPosition.Y + groupGenerator.Size.Y);

                                break;

                            case Connection.Sides.Left:
                                newMatrixPosition = new Vector2I(MatrixPosition.X - 1, MatrixPosition.Y);
                                newGlobalPosition = new Vector2I(GlobalPosition.X - groupGenerator.Size.X, GlobalPosition.Y);

                                break;

                            case Connection.Sides.Right:
                                newMatrixPosition = new Vector2I(MatrixPosition.X + 1, MatrixPosition.Y);
                                newGlobalPosition = new Vector2I(GlobalPosition.X + groupGenerator.Size.X, GlobalPosition.Y);

                                break;
                        }
                        generateRec(newMatrixPosition, newGlobalPosition, GroupsLeft);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Places a new group at the specified grid (matrix) GlobalPosition and global GlobalPosition.
    /// If the GlobalPosition is valid and empty, connections with adjacent groups are checked and used to
    /// create the new group.
    /// </summary>
    /// <param name="MatrixPosition">The grid GlobalPosition where the group is to be placed.</param>
    /// <param name="GlobalPosition">The corresponding global GlobalPosition for the new group.</param>
    /// <returns>
    /// The newly placed group if successful; otherwise, null if the GlobalPosition is out of bounds or already occupied.
    /// </returns>
    public Group placeGroup(Vector2I MatrixPosition, Vector2I GlobalPosition)
    {
        // Check if MatrixPosition is within boundaries and if that GlobalPosition is empty.
        if (MatrixPosition.X >= 0 &&
            MatrixPosition.Y >= 0 &&
            MatrixPosition.X < GeneratedPrison.Groups.GetLength(0) &&
            MatrixPosition.Y < GeneratedPrison.Groups.GetLength(1) &&
            GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y] == null)
        {
            List<Connection> Connections = new List<Connection>();

            // Check for an adjacent group above.
            if (MatrixPosition.Y != 0 && GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y - 1] != null)
            {
                if (GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y - 1].Connections.Exists(c => c.Side == Connection.Sides.Bottom))
                {
                    Connection connection = GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y - 1].Connections.Find(c => c.Side == Connection.Sides.Bottom);
                    int Height = connection.Height;
                    Connections.Add(new Connection(Connection.Sides.Top, Height, Connection.Status.Connected));
                    connection.status = Connection.Status.Connected;
                }
                else
                {
                    Connections.Add(new Connection(Connection.Sides.Top, 0, Connection.Status.Blocked));
                }
            }

            // Check for an adjacent group below.
            if (MatrixPosition.Y + 1 < GeneratedPrison.Groups.GetLength(1) && GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y + 1] != null)
            {
                if (GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y + 1].Connections.Exists(c => c.Side == Connection.Sides.Top))
                {
                    Connection connection = GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y + 1].Connections.Find(c => c.Side == Connection.Sides.Top);
                    int Height = connection.Height;
                    connection.status = Connection.Status.Connected;
                    Connections.Add(new Connection(Connection.Sides.Bottom, Height, Connection.Status.Connected));
                }
                else
                {
                    Connections.Add(new Connection(Connection.Sides.Bottom, 0, Connection.Status.Blocked));
                }
            }

            // Check for an adjacent group to the left.
            if (MatrixPosition.X != 0 && GeneratedPrison.Groups[MatrixPosition.X - 1, MatrixPosition.Y] != null)
            {
                if (GeneratedPrison.Groups[MatrixPosition.X - 1, MatrixPosition.Y].Connections.Exists(c => c.Side == Connection.Sides.Right))
                {
                    Connection connection = GeneratedPrison.Groups[MatrixPosition.X - 1, MatrixPosition.Y].Connections.Find(c => c.Side == Connection.Sides.Right);
                    int Height = connection.Height;
                    connection.status = Connection.Status.Connected;
                    Connections.Add(new Connection(Connection.Sides.Left, Height, Connection.Status.Connected));
                }
                else
                {
                    Connections.Add(new Connection(Connection.Sides.Left, 0, Connection.Status.Blocked));
                }
            }

            // Check for an adjacent group to the right.
            if (MatrixPosition.X + 1 < GeneratedPrison.Groups.GetLength(0) && GeneratedPrison.Groups[MatrixPosition.X + 1, MatrixPosition.Y] != null)
            {
                if (GeneratedPrison.Groups[MatrixPosition.X + 1, MatrixPosition.Y].Connections.Exists(c => c.Side == Connection.Sides.Left))
                {
                    Connection connection = GeneratedPrison.Groups[MatrixPosition.X + 1, MatrixPosition.Y].Connections.Find(c => c.Side == Connection.Sides.Left);
                    int Height = connection.Height;
                    connection.status = Connection.Status.Connected;
                    Connections.Add(new Connection(Connection.Sides.Right, Height, Connection.Status.Connected));
                }
                else
                {
                    Connections.Add(new Connection(Connection.Sides.Right, 0, Connection.Status.Blocked));
                }
            }

            Group NewGroup = groupGenerator.generate(GlobalPosition, Connections);
            GeneratedPrison.Groups[MatrixPosition.X, MatrixPosition.Y] = NewGroup;
            return NewGroup;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Shuffles the order of connections in the provided list.
    /// </summary>
    /// <param name="list">The list of connections to shuffle.</param>
    public void ShuffleConnectionList(List<Connection> list)
    {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Connection value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

    }

    public void PlaceExternalRooms()
    {
        possibleRooms.Shuffle();
        var seenPositions = new HashSet<Vector2>();
        possibleRooms.RemoveAll(item =>
        {
            return !seenPositions.Add(item.Item1);
        });
        foreach (var pos in possibleRooms)
        {
            Rect2I rect = new Rect2I((int)pos.Coordinates.X, (int)pos.Coordinates.Y, groupGenerator.Size);
            RoomGenerator.Position attchTo = RoomGenerator.Position.NorthWest;
            switch (pos.Position)
            {
                case Side.Left:
                    attchTo = RoomGenerator.Position.NorthWest;
                    break;
                case Side.Right:
                    attchTo = RoomGenerator.Position.NorthEast;
                    break;
                case Side.Top:
                    attchTo = RoomGenerator.Position.NorthWest;
                    break;
                case Side.Bottom:
                    attchTo = RoomGenerator.Position.SouthWest;
                    break;
            }
            
            Room newRoom = ExternalRoomGenerator.generate(rect, AttachTo: attchTo);

            if(newRoom != null)
            {
                if (pos.Position == Side.Left || pos.Position == Side.Right)
                    newRoom.GlobalPosition += new Vector2(0, (Math.Max(0, Math.Min(pos.height - newRoom.GetSizeInCells().Y / 2, groupGenerator.Size.Y - newRoom.GetSizeInCells().Y)) * Room.CellSize));

                if (pos.Position == Side.Top || pos.Position == Side.Bottom)
                    newRoom.GlobalPosition += new Vector2((Math.Max(0, Math.Min(pos.height - newRoom.GetSizeInCells().X / 2, groupGenerator.Size.X - newRoom.GetSizeInCells().X))) * Room.CellSize, 0);
                
                GeneratedPrison.ExternalRooms.Add(newRoom);
            }
            
        }
    }

    public void FixPatternBetweenGroups()
    {
        for(int x = 1; x < GeneratedPrison.Groups.GetLength(0) - 1; x++)
        {
            for (int y = 1; y < GeneratedPrison.Groups.GetLength(1) - 1; y++)
            {
                Group group = GeneratedPrison.Groups[x, y];
                if (group != null)
                {
                    foreach (Corridor corridor in group.Corridors)
                    {
                        if (corridor.GetSize().X > 0 && corridor.GetSize().Y > 0)
                        {
                            if (corridor.Area.Size.X == group.GetSize().X) //Il corridoio passa per tutta la lunghezza
                            {
                                if (GeneratedPrison.Groups[x - 1,y] != null)   
                                    corridor.bitMasks[0] |= 0b00000010;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() - new Vector2(group.GetSize().X, 0), Side.Right, (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y)));

                                if (GeneratedPrison.Groups[x + 1, y] != null)
                                    corridor.bitMasks[^1] |= 0b00100000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() + new Vector2(group.GetSize().X, 0), Side.Left, (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y)));
                            }
                            else if (corridor.Area.Size.Y == group.GetSize().Y) //Il corridoio passa per tutta l'altezza
                            {
                                if (GeneratedPrison.Groups[x, y - 1] != null)
                                    corridor.bitMasks[0] |= 0b10000000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() - new Vector2(0, group.GetSize().Y), Side.Bottom, (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X)));

                                if (GeneratedPrison.Groups[x, y + 1] != null)
                                    corridor.bitMasks[^1] |= 0b00001000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() + new Vector2(0, group.GetSize().Y), Side.Top, (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X)));
                            }
                            else if (corridor.Side == Connection.Sides.Left && corridor.GetSize().X > 0)
                            {
                                int corridorHeight = (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y);

                                if (GeneratedPrison.Groups[x - 1, y] != null)
                                    corridor.bitMasks[0] |= 0b00000010;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() - new Vector2(group.GetSize().X, 0), Side.Right, (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y)));

                                corridor.bitMasks[^1] |= 0b00100000;
                                group.Corridors[0].bitMasks[corridorHeight] |= 0b00000010;
                            }
                            else if (corridor.Side == Connection.Sides.Right && corridor.GetSize().X > 0)
                            {

                                int corridorHeight = (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y);

                                if (GeneratedPrison.Groups[x + 1, y] != null)
                                    corridor.bitMasks[^1] |= 0b00100000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() + new Vector2(group.GetSize().X, 0), Side.Left, (int)(corridor.GetGlobalPosition().Y - group.GetGlobalPosition().Y)));

                                corridor.bitMasks[0] |= 0b00000010;
                                group.Corridors[0].bitMasks[corridorHeight] |= 0b00100000;
                            }
                            else if (corridor.Side == Connection.Sides.Top && corridor.GetSize().Y > 0)
                            {
                                int corridorHeight = (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X);

                                if (GeneratedPrison.Groups[x, y - 1] != null)
                                    corridor.bitMasks[0] |= 0b10000000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() - new Vector2(0, group.GetSize().Y), Side.Bottom, (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X)));

                                corridor.bitMasks[^1] |= 0b00001000;
                                group.Corridors[0].bitMasks[corridorHeight] |= 0b10000000;
                            }
                            else if (corridor.Side == Connection.Sides.Bottom && corridor.GetSize().Y > 0)
                            {
                                int corridorHeight = (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X);

                                if (GeneratedPrison.Groups[x, y + 1] != null)
                                    corridor.bitMasks[^1] |= 0b00001000;
                                else
                                    possibleRooms.Add((group.GetGlobalPosition() + new Vector2(0, group.GetSize().Y), Side.Top, (int)(corridor.GetGlobalPosition().X - group.GetGlobalPosition().X)));

                                corridor.bitMasks[0] |= 0b10000000;
                                group.Corridors[0].bitMasks[corridorHeight] |= 0b00001000;
                            }
                        }
                    }
                }
            }
        }
    }
}