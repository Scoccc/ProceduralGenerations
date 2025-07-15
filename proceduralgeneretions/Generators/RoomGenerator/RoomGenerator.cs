using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates <see cref="Room"/> instances based on defined <see cref="RoomType"/>s and 
/// Supports weighted random selection and optional rotation of room shapes.
/// </summary>
[GlobalClass]
public partial class RoomGenerator : Generator<Room>
{
    [ExportGroup("Placing")]
    /// <summary>
    /// The maximum free space available to place the room.
    /// </summary>
    [Export] 
    public Rect2I MaxFreeSpace = new Rect2I(0, 0, 999, 999);

    /// <summary>
    /// The GlobalPosition where the room should be aligned within the free space.
    /// </summary>
    [Export] 
    public Position AttachTo = Position.NorthWest;

    /// <summary>
    /// Whether the room is allowed to be rotated (swap width and height).
    /// 
    /// </summary>
    [Export] public bool rotate = false;
    /// <summary>
    /// List of possible room types to randomly select from.
    /// </summary>
    [ExportGroup("Rooms")]
    [Export] public Array<RoomType> roomTypes;

    /// <summary>
    /// Probability of generating an empty room.
    /// </summary>
    [Export] public float emptyRoomProbability;

    [Export] public bool FreeSides_R;
    [Export] public bool FreeSides_L;
    [Export] public bool FreeSides_T;
    [Export] public bool FreeSides_B;

    RandomNumberGenerator Rng = new RandomNumberGenerator();

    private Godot.Collections.Dictionary<Side, bool> freeSides;

    private System.Collections.Generic.Dictionary<RoomType, int> roomTypeCount; //resource - numero di stanze generate di quella resource

    public enum Position
    {
        /// <summary>Attach room to the top-right corner.</summary>
        NorthEast,

        /// <summary>Attach room to the top-left corner.</summary>
        NorthWest,

        /// <summary>Attach room to the bottom-left corner.</summary>
        SouthWest,

        /// <summary>Attach room to the bottom-right corner.</summary>
        SouthEast,

        /// <summary>Place room centered within the space.</summary>
        Centered
    }

    public RoomGenerator()
    {
        roomTypeCount = new System.Collections.Generic.Dictionary<RoomType, int>();
    }

    /// <summary>
    /// Generates a room using the specified <paramref name="generationSettings"/>.
    /// Chooses a room type based on weighted probabilities and fits it within the available space.
    /// </summary>
    /// <param name="generationSettings">The generation constraints, such as maximum free space and GlobalPosition.</param>
    /// <returns>A generated <see cref="Room"/> or null if no suitable room can be placed.</returns>
    /// 
    public Room generate(Rect2I MaxFreeSpace, bool rightSide = false, bool leftSide = false, bool topSide = false, bool bottomSide = false, Position AttachTo = Position.NorthWest, bool rotate = false)
    {
        this.MaxFreeSpace = new Rect2I(MaxFreeSpace.Position.X * Room.CellSize,
                                       MaxFreeSpace.Position.Y * Room.CellSize,
                                       MaxFreeSpace.Size.X * Room.CellSize,
                                       MaxFreeSpace.Size.Y * Room.CellSize);
        this.AttachTo = AttachTo;
        freeSides = new Godot.Collections.Dictionary<Side, bool>();
        freeSides.Add(Side.Right, rightSide);
        freeSides.Add(Side.Left, leftSide);
        freeSides.Add(Side.Top, topSide);
        freeSides.Add(Side.Bottom, bottomSide);
        this.rotate = rotate;

        return generate();
    }

    private RoomType GetRandomRoomType(List<RoomType> elements)
    {
        Rng.Randomize();

        float TotalWeight = elements.Sum(room => room.SpawnProbability) + emptyRoomProbability;
        float RandomValue = Rng.RandfRange(0, TotalWeight);
        float CumulativeWeight = 0;

        foreach (RoomType CurrentRoomType in elements)
        {
            CumulativeWeight += CurrentRoomType.SpawnProbability;
            if (RandomValue < CumulativeWeight)
            {
                return CurrentRoomType;
            }
        }

        return null;
    }

    public override Room generate()
    {
        if(freeSides == null)
        {
            freeSides = new Godot.Collections.Dictionary<Side, bool>();
            freeSides.Add(Side.Right, FreeSides_R);
            freeSides.Add(Side.Left, FreeSides_L);
            freeSides.Add(Side.Top, FreeSides_T);
            freeSides.Add(Side.Bottom, FreeSides_B);
        }

        List<RoomType> ValidRoomTypes = roomTypes.ToList();

        while (ValidRoomTypes.Count > 0)
        {
            RoomType SelectedRoomType = GetRandomRoomType(ValidRoomTypes);

            if (SelectedRoomType == null) return null;

            if (!roomTypeCount.ContainsKey(SelectedRoomType))
            {
                roomTypeCount.Add(SelectedRoomType, 0);
            }

            if (!SelectedRoomType.ignoreMaxNumber && roomTypeCount[SelectedRoomType] >= SelectedRoomType.MaxNumber)
            {
                ValidRoomTypes.Remove(SelectedRoomType);
                roomTypes.Remove(SelectedRoomType);
            }
            else
            {
                Room orientatedRoom = GetOrientatedRoom(SelectedRoomType);

                if (orientatedRoom != null) return orientatedRoom;
                else ValidRoomTypes.Remove(SelectedRoomType);
            }
            
        }

        return null;
    }

    private Room GetOrientatedRoom(RoomType SelectedRoomType)
    {
        Vector2I ActualMinSize = SelectedRoomType.MaxSizeCell * Room.CellSize;

        if (!rotate)
        {
            if (ActualMinSize.X <= MaxFreeSpace.Size.X && ActualMinSize.Y <= MaxFreeSpace.Size.Y)
            {
                Vector2I RoomSize = new Vector2I(
                    Rng.RandiRange(SelectedRoomType.MinSizeCell.X, Math.Min(SelectedRoomType.MaxSizeCell.X, MaxFreeSpace.Size.X / Room.CellSize)),
                    Rng.RandiRange(SelectedRoomType.MinSizeCell.Y, Math.Min(SelectedRoomType.MaxSizeCell.Y, MaxFreeSpace.Size.Y / Room.CellSize))
                );

                Vector2I RoomPosition = CalculateRoomPosition(RoomSize);

                ++roomTypeCount[SelectedRoomType];
                return new Room(RoomPosition, RoomSize, SelectedRoomType.Color, freeSides);

            }
            else
            {
                return null;
            }
        }
        else
        {
            if (ActualMinSize.Y <= MaxFreeSpace.Size.X && ActualMinSize.X <= MaxFreeSpace.Size.Y)
            {
                Vector2I RoomSize = new Vector2I(
                    Rng.RandiRange(SelectedRoomType.MinSizeCell.Y, Math.Min(SelectedRoomType.MaxSizeCell.Y, MaxFreeSpace.Size.X / Room.CellSize)),
                    Rng.RandiRange(SelectedRoomType.MinSizeCell.X, Math.Min(SelectedRoomType.MaxSizeCell.X, MaxFreeSpace.Size.Y / Room.CellSize))
                );

                Vector2I RoomPosition = CalculateRoomPosition(RoomSize);

                if (SelectedRoomType.Color == Colors.Transparent)
                {
                    return null;
                }
                else
                {
                    ++roomTypeCount[SelectedRoomType];
                    return new Room(RoomPosition, RoomSize, SelectedRoomType.Color, freeSides);
                }
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Calculates the GlobalPosition and size of a room based on alignment 
    /// </summary>
    /// <param name="roomSize">The dimensions of the room.</param>
    /// <param name="settings">The generation </param>
    /// <returns>A <see cref="Rect2I"/> defining the room's CellsArea in space.</returns>
    private Vector2I CalculateRoomPosition(Vector2I RoomSize)
    {
        switch (AttachTo)
        {
            case Position.SouthEast:
                return new Vector2I(
                    MaxFreeSpace.Position.X + (MaxFreeSpace.Size.X - RoomSize.X * Room.CellSize),
                    MaxFreeSpace.Position.Y + (MaxFreeSpace.Size.Y - RoomSize.Y * Room.CellSize));

            case Position.SouthWest:
                return new Vector2I(
                    MaxFreeSpace.Position.X,
                    MaxFreeSpace.Position.Y + (MaxFreeSpace.Size.Y - RoomSize.Y * Room.CellSize));

            case Position.NorthEast:
                return new Vector2I(
                    MaxFreeSpace.Position.X + (MaxFreeSpace.Size.X - RoomSize.X * Room.CellSize),
                    MaxFreeSpace.Position.Y);

            case Position.Centered:
                return new Vector2I(
                    MaxFreeSpace.Position.X + ((MaxFreeSpace.Size.X - RoomSize.X * Room.CellSize) / 2),
                    MaxFreeSpace.Position.Y + ((MaxFreeSpace.Size.Y - RoomSize.Y * Room.CellSize) / 2));

            case Position.NorthWest:
            default:
                return new Vector2I(MaxFreeSpace.Position.X, MaxFreeSpace.Position.Y);
        }
    }
}


