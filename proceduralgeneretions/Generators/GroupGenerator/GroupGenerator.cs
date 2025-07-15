using Godot;
using System;
using System.Collections.Generic;
[GlobalClass]
public partial class GroupGenerator : Generator<Group>
{
    [ExportGroup("Group")]
    [ExportSubgroup("Size")]
    [Export]
    public Vector2I Size;

    /// <summary>
    /// The minimum subdivision size required for generating rooms inside.
    /// The generated area must be at least as big as (MinSubdivionSize * 2 + 1).
    /// I corridoi verticali spawneranno con altezza compresa tra MinSubdivionSize.X e GrooupSize.X - MinSubdivionSize.X,
    /// quindi se MinSubdivionSize.X vale 0 i corridoi verticali potranno spawnare anche attaccati ai lati del gruppi.
    /// Stessa cosa vale per quelli orizzontali uando le Y
    /// </summary>
    [Export] public Vector2I MinSubdivionSize;
    [ExportGroup("Rooms")]

    /// <summary>
    /// The room generator used to create individual rooms inside the subdivisions.
    /// </summary>
    [Export]public RoomGenerator RoomGenerator;

    [ExportGroup("Corridors")]
    /// <summary>
    /// The probability (between 0 and 1) of splitting a subdivision during group generation.
    /// If the value is 0, the group will have only one corridor.
    /// If the value is 1, the group will have exactly 4 corridors.
    /// </summary>
    [Export(PropertyHint.Range, "0,1,0.01")] public float SplitChance;

    [ExportSubgroup("Offset")]
    /// <summary>
    /// The maximum offset (as a percentage) from the center used for splitting subdivisions placing a corridor between.
    /// Value ranges between 0 and 1.
    /// If the value is 0, corridors will be placed always in the center.
    /// If the value is 1, corridors can be placed in a random height between MinSubdivisionSize and GetSize - MinSubdivisionSize.
    /// </summary>
    [Export(PropertyHint.Range, "0,1,0.01")] public float MaxOffsetFromCenterPercentage;

    private RandomNumberGenerator Rng = new RandomNumberGenerator();
    private List<Rect2I> SpacesBetweenCorridors;
    private int MaxOffsetFromCenter = 0;
    private Vector2I Position;
    private List<Connection> Connections = null;

    /// <summary>
    /// Exception thrown when the generation parameters are invalid.
    /// </summary>
    private class GenerationParamError : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationParamError"/> class with a specified error message.
        /// </summary>
        /// <param name="Error">The error message describing the parameter issue.</param>
        public GenerationParamError(string Error) : base(Error) { }
    }

    public Group generate(Vector2I Position, List<Connection> Connections = null)
    {
        this.Position = Position;
        this.Connections = Connections;

        return generate();
    }

    /// <summary>
    /// Generates a group starting at the specified GlobalPosition, optionally using an existing list of connections.
    /// If the provided list of <see cref="Connection"/> is null, it indicates that there are no adjacent groups,
    /// and the group is generated independently. Otherwise, the generated group will be connected to adjacent groups
    /// by aligning its corridor with the existing connections.
    /// </summary>
    /// <param name="Position">The starting GlobalPosition for the group's area.</param>
    /// <param name="Connections">An optional list of existing connections; if null, a new list is created.</param>
    /// <returns>A generated <see cref="Group"/> containing rooms, corridors, and connections.</returns>
    public override Group generate()
    {
        Rect2I Area = new Rect2I(Position, Size);
        if (Area.Size.X < MinSubdivionSize.X * 2 + 1 || Area.Size.Y < MinSubdivionSize.Y * 2 + 1)
            throw new GenerationParamError("The Area is too small; it must be at least as big as MinSubdivionSize * 2 + 1");

        Group GeneratedGroup = new Group(Area);
        SpacesBetweenCorridors = new List<Rect2I>();
        SpacesBetweenCorridors.Add(Area);

        int CorridorHeight;
        Rng.Randomize();

        if (Connections == null)
            Connections = new List<Connection>();

        int SideDirection;
        int AreaCenter;
        int MinCorridorHeight;
        int MaxCorridorHeight;

        if (Connections.Count == 0)
        {
            Connection.Sides RandomSide = Connection.GetRandomSide();
            SideDirection = RandomSide == Connection.Sides.Top || RandomSide == Connection.Sides.Bottom ? 0 : 1;
            AreaCenter = Area.Size[SideDirection] / 2;
            MaxOffsetFromCenter = (int)(MaxOffsetFromCenterPercentage * (Area.Size[SideDirection] / 2));
            MinCorridorHeight = Math.Max(MinSubdivionSize[SideDirection], AreaCenter - MaxOffsetFromCenter);
            MaxCorridorHeight = Math.Min(Area.Size[SideDirection] - MinSubdivionSize[SideDirection] - 1, AreaCenter + MaxOffsetFromCenter);

            CorridorHeight = Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight);

            Connection FirstSplit = new Connection(RandomSide, CorridorHeight);
            Connections.Add(FirstSplit);
        }

        if (Connections.Count == 1)
        {
            if (Rng.Randf() < SplitChance)
            {
                Connection SecondSplit;

                if (Connections[0].GetDirection() == 1)
                {
                    MaxOffsetFromCenter = (int)(MaxOffsetFromCenterPercentage * (Area.Size.X / 2));
                    AreaCenter = Area.Size.X / 2;
                    MinCorridorHeight = Math.Max(MinSubdivionSize.X, AreaCenter - MaxOffsetFromCenter);
                    MaxCorridorHeight = Math.Min(Area.Size.X - MinSubdivionSize.X - 1, AreaCenter + MaxOffsetFromCenter);

                    if (Rng.RandiRange(0, 1) == 0)
                    {
                        SecondSplit = new Connection(Connection.Sides.Top, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                    }
                    else
                    {
                        SecondSplit = new Connection(Connection.Sides.Bottom, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                    }
                }
                else
                {
                    MaxOffsetFromCenter = (int)(MaxOffsetFromCenterPercentage * (Area.Size.Y / 2));
                    AreaCenter = Area.Size.Y / 2;
                    MinCorridorHeight = Math.Max(MinSubdivionSize.Y, AreaCenter - MaxOffsetFromCenter);
                    MaxCorridorHeight = Math.Min(Area.Size.Y - MinSubdivionSize.Y - 1, AreaCenter + MaxOffsetFromCenter);

                    if (Rng.RandiRange(0, 1) == 0)
                    {
                        SecondSplit = new Connection(Connection.Sides.Left, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                    }
                    else
                    {
                        SecondSplit = new Connection(Connection.Sides.Right, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                    }
                }
                Connections.Add(SecondSplit);
            }
        }

        if (Connections.Count == 2)
        {
            if (Rng.Randf() < SplitChance)
            {
                Connection ThirdSplit;

                if (Connections[0].GetDirection() == 1)
                {
                    MaxOffsetFromCenter = (int)(MaxOffsetFromCenterPercentage * (Area.Size.X / 2));
                    AreaCenter = Area.Size.X / 2;
                    MinCorridorHeight = Math.Max(MinSubdivionSize.X, AreaCenter - MaxOffsetFromCenter);
                    MaxCorridorHeight = Math.Min(Area.Size.X - MinSubdivionSize.X - 1, AreaCenter + MaxOffsetFromCenter);

                    if (Connections[1].Side == Connection.Sides.Bottom && !Connections.Exists(c => c.Side == Connection.Sides.Top && c.status == Connection.Status.Blocked))
                    {
                        ThirdSplit = new Connection(Connection.Sides.Top, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                        Connections.Add(ThirdSplit);
                    }
                    else if (Connections[1].Side == Connection.Sides.Top && !Connections.Exists(c => c.Side == Connection.Sides.Bottom && c.status == Connection.Status.Blocked))
                    {
                        ThirdSplit = new Connection(Connection.Sides.Bottom, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                        Connections.Add(ThirdSplit);
                    }
                }
                else
                {
                    MaxOffsetFromCenter = (int)(MaxOffsetFromCenterPercentage * (Area.Size.Y / 2));
                    AreaCenter = Area.Size.Y / 2;
                    MinCorridorHeight = Math.Max(MinSubdivionSize.Y, AreaCenter - MaxOffsetFromCenter);
                    MaxCorridorHeight = Math.Min(Area.Size.Y - MinSubdivionSize.Y - 1, AreaCenter + MaxOffsetFromCenter);

                    if (Connections[1].Side == Connection.Sides.Right && !Connections.Exists(c => c.Side == Connection.Sides.Left && c.status == Connection.Status.Blocked))
                    {
                        ThirdSplit = new Connection(Connection.Sides.Left, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                        Connections.Add(ThirdSplit);
                    }
                    else if (Connections[1].Side == Connection.Sides.Left && !Connections.Exists(c => c.Side == Connection.Sides.Right && c.status == Connection.Status.Blocked))
                    {
                        ThirdSplit = new Connection(Connection.Sides.Right, Rng.RandiRange(MinCorridorHeight, MaxCorridorHeight));
                        Connections.Add(ThirdSplit);
                    }
                }
            }

        }

        foreach (Connection Connection in Connections)
        {
            MakeCorridor(GeneratedGroup, Connection);
        }

        foreach (Rect2I Space in SpacesBetweenCorridors)
        {
            PlaceRooms(GeneratedGroup, Space);
        }


        return GeneratedGroup;
    }

    /// <summary>
    /// Creates a corridor within the specified group based on a starting connection.
    /// This method validates the connection, computes the corridor's area,
    /// and updates the group's connections and corridors accordingly.
    /// </summary>
    /// <param name="Group">The group to which the corridor will be added.</param>
    /// <param name="start">The starting connection used to determine corridor placement.</param>
    public void MakeCorridor(Group Group, Connection start)
    {
        // Check if connection is valid.
        if (start.Height < 0 || start.Height >= Group.GetSize()[start.GetDirection()]) return;

        Rect2I Area = new Rect2I();
        if (Group.Corridors.Count == 0) // First group.
        {
            if (start.Side == Connection.Sides.Right)
            {
                Area = new Rect2I((int)Group.GetGlobalPosition().X, (int)Group.GetGlobalPosition().Y + start.Height,Group.GetSize().X, 1);
                Group.Connections.Add(new Connection(Connection.Sides.Right, start.Height, start.status));
                Group.Connections.Add(new Connection(Connection.Sides.Left, start.Height));
            }
            else if (start.Side == Connection.Sides.Left)
            {
                Area = new Rect2I((int)Group.GetGlobalPosition().X, (int)Group.GetGlobalPosition().Y + start.Height,Group.GetSize().X, 1);
                Group.Connections.Add(new Connection(Connection.Sides.Right, start.Height));
                Group.Connections.Add(new Connection(Connection.Sides.Left, start.Height, start.status));
            }
            else if (start.Side == Connection.Sides.Top)
            {
                Area = new Rect2I((int)Group.GetGlobalPosition().X + start.Height, (int)Group.GetGlobalPosition().Y, 1,Group.GetSize().Y);
                Group.Connections.Add(new Connection(Connection.Sides.Top, start.Height, start.status));
                Group.Connections.Add(new Connection(Connection.Sides.Bottom, start.Height));
            }
            else
            {
                Area = new Rect2I((int)Group.GetGlobalPosition().X + start.Height, (int)Group.GetGlobalPosition().Y, 1,Group.GetSize().Y);
                Group.Connections.Add(new Connection(Connection.Sides.Top, start.Height));
                Group.Connections.Add(new Connection(Connection.Sides.Bottom, start.Height, start.status));
            }
        }
        else // The group must be connected.
        {
            Corridor PerpendicularCorridor = Group.Corridors.Find(c => c.GetDirection() == start.GetDirection());
            if (PerpendicularCorridor != null)
            {
                if (start.Side == Connection.Sides.Left)
                {
                    int Lenght =(int)(PerpendicularCorridor.GetGlobalPosition().X - Group.GetGlobalPosition().X);
                    Area = new Rect2I((int)Group.GetGlobalPosition().X, (int)Group.GetGlobalPosition().Y + start.Height, Lenght, 1);
                    Group.Connections.Add(new Connection(Connection.Sides.Left, start.Height));
                }
                else if (start.Side == Connection.Sides.Right)
                {
                    int Lenght = (int)(Group.GetGlobalPosition().X + Group.GetSize().X - PerpendicularCorridor.GetGlobalPosition().X - 1);
                    Area = new Rect2I((int)PerpendicularCorridor.GetGlobalPosition().X + 1, (int)Group.GetGlobalPosition().Y + start.Height, Lenght, 1);
                    Group.Connections.Add(new Connection(Connection.Sides.Right, start.Height));
                }
                else if (start.Side == Connection.Sides.Top)
                {
                    int Lenght = (int)(PerpendicularCorridor.GetGlobalPosition().Y - Group.GetGlobalPosition().Y);
                    Area = new Rect2I((int)Group.GetGlobalPosition().X + start.Height, (int)Group.GetGlobalPosition().Y, 1, Lenght);
                    Group.Connections.Add(new Connection(Connection.Sides.Top, start.Height));
                }
                else if (start.Side == Connection.Sides.Bottom)
                {
                    int Lenght = (int)(Group.GetGlobalPosition().Y + Group.GetSize().Y - PerpendicularCorridor.GetGlobalPosition().Y - 1);
                    Area = new Rect2I((int)PerpendicularCorridor.GetGlobalPosition().X + start.Height, (int)PerpendicularCorridor.GetGlobalPosition().Y + 1, 1, Lenght);
                    Group.Connections.Add(new Connection(Connection.Sides.Bottom, start.Height));
                }
            }
        }

        Corridor NewCorridor = new Corridor(start.Side, Area);
        SplitSpace(NewCorridor);
        Group.AddCorridor(NewCorridor);
    }

    /// <summary>
    /// Splits the free space in the group based on the area occupied by a corridor.
    /// The original free space is divided into two parts and updated accordingly.
    /// </summary>
    /// <param name="Corridor">The corridor whose area is used to split the free space.</param>
    public void SplitSpace(Corridor Corridor)
    {
        Rect2I SpaceToSplit = SpacesBetweenCorridors.Find(e => e.Intersects(Corridor.Area));
        Rect2I FirstHalf;
        Rect2I SecondHalf;

        if (Corridor.Side == Connection.Sides.Right || Corridor.Side == Connection.Sides.Left)
        {
            FirstHalf = new Rect2I(SpaceToSplit.Position.X, SpaceToSplit.Position.Y, SpaceToSplit.Size.X, (int)Corridor.GetGlobalPosition().Y - SpaceToSplit.Position.Y);
            SecondHalf = new Rect2I(SpaceToSplit.Position.X, (int)Corridor.GetGlobalPosition().Y + 1, SpaceToSplit.Size.X, SpaceToSplit.Size.Y - (int)Corridor.GetGlobalPosition().Y + SpaceToSplit.Position.Y - 1);
        }
        else
        {
            FirstHalf = new Rect2I(SpaceToSplit.Position.X, SpaceToSplit.Position.Y, (int)Corridor.GetGlobalPosition().X - SpaceToSplit.Position.X, SpaceToSplit.Size.Y);
            SecondHalf = new Rect2I((int)Corridor.GetGlobalPosition().X + 1, SpaceToSplit.Position.Y, SpaceToSplit.Size.X - (int)Corridor.GetGlobalPosition().X + SpaceToSplit.Position.X - 1, SpaceToSplit.Size.Y);
        }

        SpacesBetweenCorridors.Remove(SpaceToSplit);
        SpacesBetweenCorridors.Add(FirstHalf);
        SpacesBetweenCorridors.Add(SecondHalf);
    }

    /// <summary>
    /// Fills the specified free space with rooms.
    /// Rooms are generated attached to the adjacent corridors using the RoomGenerator and then added to the group.
    /// </summary>
    /// <param name="Group">The group to which the rooms will be added.</param>
    /// <param name="FreeSpace">The rectangular area representing the free space to fill with rooms.</param>

    public void PlaceRooms(Group Group, Rect2I FreeSpace)
    {
        if (FreeSpace.Size.X > 0 && FreeSpace.Size.Y > 0)
        {
            bool isBottom = FreeSpace.Position.Y > Group.GetGlobalPosition().Y;
            bool isRight = FreeSpace.Position.X > Group.GetGlobalPosition().X;

            if (isBottom && FreeSpace.Size.X ==Group.GetSize().X)
            {
                int SpaceLeft = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X;
                while (SpaceLeft > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(xPos, FreeSpace.Position.Y, SpaceLeft, FreeSpace.Size.Y);
                    Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, topSide: true);
                    if (GeneratedRoom != null)
                    {
                        Group.AddRoom(GeneratedRoom);
                        SpaceLeft -= GeneratedRoom.GetSizeInCells().X;
                        xPos += GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeft -= 1;
                        xPos += 1;
                    }
                }
            }
            else if (!isBottom && FreeSpace.Size.X ==Group.GetSize().X)
            {
                int SpaceLeft = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X;
                while (SpaceLeft > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(xPos, FreeSpace.Position.Y, SpaceLeft, FreeSpace.Size.Y);
                    Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, bottomSide:true, AttachTo:RoomGenerator.Position.SouthWest);
                    if (GeneratedRoom != null)
                    {
                        Group.AddRoom(GeneratedRoom);
                        SpaceLeft -= GeneratedRoom.GetSizeInCells().X;
                        xPos += GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeft -= 1;
                        xPos += 1;
                    }
                }
            }
            else if (isRight && FreeSpace.Size.Y ==Group.GetSize().Y)
            {
                int SpaceLeft = FreeSpace.Size.Y;
                int yPos = FreeSpace.Position.Y;
                while (SpaceLeft > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(FreeSpace.Position.X, yPos, FreeSpace.Size.X, SpaceLeft);
                    Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, leftSide:true, AttachTo:RoomGenerator.Position.NorthWest, rotate:true);
                    if (GeneratedRoom != null)
                    {
                        Group.AddRoom(GeneratedRoom);
                        SpaceLeft -= GeneratedRoom.GetSizeInCells().Y;
                        yPos += GeneratedRoom.GetSizeInCells().Y;
                    }
                    else
                    {
                        SpaceLeft -= 1;
                        yPos += 1;
                    }
                }
            }
            else if (!isRight && FreeSpace.Size.Y ==Group.GetSize().Y)
            {
                int SpaceLeft = FreeSpace.Size.Y;
                int yPos = FreeSpace.Position.Y;
                while (SpaceLeft > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(FreeSpace.Position.X, yPos, FreeSpace.Size.X, SpaceLeft);
                    Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, rightSide:true,AttachTo: RoomGenerator.Position.NorthEast, rotate:true);
                    if (GeneratedRoom != null)
                    {
                        Group.AddRoom(GeneratedRoom);
                        SpaceLeft -= GeneratedRoom.GetSizeInCells().Y;
                        yPos += GeneratedRoom.GetSizeInCells().Y;
                    }
                    else
                    {
                        SpaceLeft -= 1;
                        yPos += 1;
                    }
                }
            }
            else if (isBottom && isRight)
            {
                bool[,] isFilled = new bool[FreeSpace.Size.X, FreeSpace.Size.Y];
                int SpaceLeftX = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X;
                while (SpaceLeftX > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(xPos, FreeSpace.Position.Y, SpaceLeftX, FreeSpace.Size.Y);
                    Room GeneratedRoom;
                    if (xPos == FreeSpace.Position.X)
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, leftSide:true, topSide: true);
                    else
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, topSide: true);
                    if (GeneratedRoom != null)
                    {
                        for (int i = 0; i < GeneratedRoom.GetSizeInCells().X; i++)
                            for (int j = 0; j < GeneratedRoom.GetSizeInCells().Y; j++)
                                isFilled[(int)GeneratedRoom.GetGlobalPosition().X / Room.CellSize - FreeSpace.Position.X + i, (int)GeneratedRoom.GetGlobalPosition().Y / Room.CellSize - FreeSpace.Position.Y + j] = true;

                        Group.AddRoom(GeneratedRoom);
                        SpaceLeftX -= GeneratedRoom.GetSizeInCells().X;
                        xPos += GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeftX -= 1;
                        xPos += 1;
                    }
                }

                xPos = FreeSpace.Position.X;
                int yPos = FreeSpace.Position.Y;
                for (int y = 0; y < isFilled.GetLength(1); y++)
                {
                    if (!isFilled[0, y])
                    {
                        SpaceLeftX = 0;
                        while (SpaceLeftX < isFilled.GetLength(0) && !isFilled[SpaceLeftX, y])
                        {
                            SpaceLeftX++;
                        }
                        Rect2I NewFreeSpace = new Rect2I(xPos, yPos, SpaceLeftX, FreeSpace.Size.Y - y);
                        Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, leftSide: true, rotate: true);
                        if (GeneratedRoom != null)
                        {
                            Group.AddRoom(GeneratedRoom);
                            yPos += GeneratedRoom.GetSizeInCells().Y;
                            y += GeneratedRoom.GetSizeInCells().Y - 1;
                        }
                        else
                        {
                            yPos += 1;
                        }
                    }
                    else
                    {
                        yPos++;
                    }
                }
            }
            else if (!isBottom && isRight)
            {
                bool[,] isFilled = new bool[FreeSpace.Size.X, FreeSpace.Size.Y];
                int SpaceLeftX = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X;
                while (SpaceLeftX > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(xPos, FreeSpace.Position.Y, SpaceLeftX, FreeSpace.Size.Y);
                    Room GeneratedRoom;
                    if (xPos == FreeSpace.Position.X)
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, leftSide:true, bottomSide: true, AttachTo: RoomGenerator.Position.SouthWest);
                    else
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, bottomSide: true, AttachTo: RoomGenerator.Position.SouthWest);
                    if (GeneratedRoom != null)
                    {
                        for (int i = 0; i < GeneratedRoom.GetSizeInCells().X; i++)
                            for (int j = 0; j < GeneratedRoom.GetSizeInCells().Y; j++)
                                isFilled[(int)GeneratedRoom.GetGlobalPosition().X / Room.CellSize - FreeSpace.Position.X + i, (int)GeneratedRoom.GetGlobalPosition().Y / Room.CellSize - FreeSpace.Position.Y + j] = true;

                        Group.AddRoom(GeneratedRoom);
                        SpaceLeftX -= GeneratedRoom.GetSizeInCells().X;
                        xPos += GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeftX -= 1;
                        xPos += 1;
                    }
                }

                xPos = FreeSpace.Position.X;
                int yPos = FreeSpace.Position.Y + FreeSpace.Size.Y - 1;
                for (int y = isFilled.GetLength(1) - 1; y >= 0; y--)
                {
                    if (!isFilled[0, y])
                    {
                        SpaceLeftX = 0;
                        while (SpaceLeftX < isFilled.GetLength(0) && !isFilled[SpaceLeftX, y])
                        {
                            SpaceLeftX++;
                        }
                        Rect2I NewFreeSpace = new Rect2I(xPos, FreeSpace.Position.Y, SpaceLeftX, y + 1);
                        Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, leftSide:true, AttachTo: RoomGenerator.Position.SouthWest, rotate: true);
                        if (GeneratedRoom != null)
                        {
                            Group.AddRoom(GeneratedRoom);
                            yPos -= GeneratedRoom.GetSizeInCells().Y;
                            y -= GeneratedRoom.GetSizeInCells().Y - 1;
                        }
                        else
                        {
                            yPos -= 1;
                        }
                    }
                    else
                    {
                        yPos--;
                    }
                }
            }
            else if (isBottom && !isRight)
            {
                bool[,] isFilled = new bool[FreeSpace.Size.X, FreeSpace.Size.Y];
                int SpaceLeftX = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X + FreeSpace.Size.X - 1;
                while (SpaceLeftX > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(FreeSpace.Position.X, FreeSpace.Position.Y, SpaceLeftX, FreeSpace.Size.Y);
                    Room GeneratedRoom;
                    if (xPos == FreeSpace.Position.X + FreeSpace.Size.X - 1)
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, rightSide:true, topSide: true, AttachTo: RoomGenerator.Position.NorthEast);
                    else
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, topSide: true, AttachTo: RoomGenerator.Position.NorthEast);
                    if (GeneratedRoom != null)
                    {
                        for (int i = 0; i < GeneratedRoom.GetSizeInCells().X; i++)
                            for (int j = 0; j < GeneratedRoom.GetSizeInCells().Y; j++)
                                isFilled[(int)GeneratedRoom.GetGlobalPosition().X / Room.CellSize - FreeSpace.Position.X + i, (int)GeneratedRoom.GetGlobalPosition().Y / Room.CellSize - FreeSpace.Position.Y + j] = true;

                        Group.AddRoom(GeneratedRoom);
                        SpaceLeftX -= GeneratedRoom.GetSizeInCells().X;
                        xPos -= GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeftX -= 1;
                        xPos -= 1;
                    }
                }

                xPos = FreeSpace.Position.X + FreeSpace.Size.X - 1;
                int yPos = FreeSpace.Position.Y;
                for (int y = 0; y < isFilled.GetLength(1); y++)
                {
                    if (!isFilled[isFilled.GetLength(0) - 1, y])
                    {
                        SpaceLeftX = 0;
                        while (SpaceLeftX < isFilled.GetLength(0) && !isFilled[isFilled.GetLength(0) - 1 - SpaceLeftX, y])
                        {
                            SpaceLeftX++;
                        }
                        Rect2I NewFreeSpace = new Rect2I(xPos - SpaceLeftX + 1, yPos, SpaceLeftX, FreeSpace.Size.Y - y);
                        Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, rightSide:true, AttachTo: RoomGenerator.Position.NorthEast, rotate: true);
                        if (GeneratedRoom != null)
                        {
                            Group.AddRoom(GeneratedRoom);
                            yPos += GeneratedRoom.GetSizeInCells().Y;
                            y += GeneratedRoom.GetSizeInCells().Y - 1;
                        }
                        else
                        {
                            yPos += 1;
                        }
                    }
                    else
                    {
                        yPos++;
                    }
                }
            }
            else
            {
                bool[,] isFilled = new bool[FreeSpace.Size.X, FreeSpace.Size.Y];
                int SpaceLeftX = FreeSpace.Size.X;
                int xPos = FreeSpace.Position.X + FreeSpace.Size.X - 1;
                while (SpaceLeftX > 0)
                {
                    Rect2I NewFreeSpace = new Rect2I(FreeSpace.Position.X, FreeSpace.Position.Y, SpaceLeftX, FreeSpace.Size.Y);
                    Room GeneratedRoom;
                    if (xPos == FreeSpace.Position.X + FreeSpace.Size.X - 1)
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, rightSide:true, bottomSide: true, AttachTo: RoomGenerator.Position.SouthEast);
                    else
                        GeneratedRoom = RoomGenerator.generate(NewFreeSpace, bottomSide:true, AttachTo: RoomGenerator.Position.SouthEast);
                    if (GeneratedRoom != null)
                    {
                        for (int i = 0; i < GeneratedRoom.GetSizeInCells().X; i++)
                            for (int j = 0; j < GeneratedRoom.GetSizeInCells().Y; j++)
                                isFilled[(int)GeneratedRoom.GetGlobalPosition().X / Room.CellSize - FreeSpace.Position.X + i, (int)GeneratedRoom.GetGlobalPosition().Y / Room.CellSize - FreeSpace.Position.Y + j] = true;

                        Group.AddRoom(GeneratedRoom);
                        SpaceLeftX -= GeneratedRoom.GetSizeInCells().X;
                        xPos -= GeneratedRoom.GetSizeInCells().X;
                    }
                    else
                    {
                        SpaceLeftX -= 1;
                        xPos -= 1;
                    }
                }

                xPos = FreeSpace.Position.X + FreeSpace.Size.X - 1;
                int yPos = FreeSpace.Position.Y + FreeSpace.Size.Y - 1;
                for (int y = isFilled.GetLength(1) - 1; y >= 0; y--)
                {
                    if (!isFilled[isFilled.GetLength(0) - 1, y])
                    {
                        SpaceLeftX = 0;
                        while (SpaceLeftX < isFilled.GetLength(0) && !isFilled[isFilled.GetLength(0) - 1 - SpaceLeftX, y])
                        {
                            SpaceLeftX++;
                        }
                        Rect2I NewFreeSpace = new Rect2I(xPos - SpaceLeftX + 1, FreeSpace.Position.Y, SpaceLeftX, y + 1);
                        Room GeneratedRoom = RoomGenerator.generate(NewFreeSpace, rightSide:true, AttachTo: RoomGenerator.Position.SouthEast, rotate:true);
                        if (GeneratedRoom != null)
                        {
                            Group.AddRoom(GeneratedRoom);
                            yPos -= GeneratedRoom.GetSizeInCells().Y;
                            y -= GeneratedRoom.GetSizeInCells().Y - 1;
                        }
                        else
                        {
                            yPos -= 1;
                        }
                    }
                    else
                    {
                        yPos--;
                    }
                }
            }
        }
    }
}
