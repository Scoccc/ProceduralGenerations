using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
/// <summary>
/// Represents a room type with configurable appearance, spawn probability, and size constraints.
/// This class is used by the room Generator to create rooms with varying characteristics.
/// </summary>
public partial class RoomType : Resource
{
    [Export] public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Color associated with this room type.
    /// </summary>
    [Export] public Color Color { get; set; }

    [ExportGroup("Spawn")]

    [Export] public bool ignoreMaxNumber = true;
    ///<summary>
    /// The max number of rooms that could spawn of this type
    ///</summary>
    [Export(PropertyHint.Range, "0,20, 1, or_greater")] public int MaxNumber = 999;

    /// <summary>
    /// Gets or sets the spawn probability for this room type.
    /// This value influences the likelihood of this type being selected during room generation.
    /// If this value is 0 this RoomType can't spawn
    /// </summary>
    [Export] public float SpawnProbability { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowable size for rooms of this type.
    /// </summary>
    [ExportGroup("Possible Size")]
    [Export] public Vector2I MinSizeCell { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowable size for rooms of this type.
    /// </summary>
    [Export] public Vector2I MaxSizeCell { get; set; }

    /// <summary>
    /// Exception thrown when the parameters for a room type are invalid,
    /// for example, if the minimum size exceeds the maximum size.
    /// </summary>
    public class InvalidParam : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParam"/> class with the specified room type name.
        /// </summary>
        /// <param name="RoomTypeName">The name of the room type that caused the exception.</param>
        public InvalidParam(string RoomTypeName) : base(RoomTypeName) { }
    }

    public class InsufficentNumber : Exception
    {
        public InsufficentNumber(string RoomTypeName) : base(RoomTypeName) { }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomType"/> class.
    /// Sets default values for the Color, spawn probability, and size constraints.
    /// Throws an <see cref="InvalidParam"/> exception if the minimum size exceeds the maximum size.
    /// </summary>
    public RoomType()
    {
        Color = Colors.Black;
        SpawnProbability = 0;
        if (MinSizeCell.X > MaxSizeCell.X || MinSizeCell.Y > MaxSizeCell.Y)
            throw new InvalidParam("Room");

        MinSizeCell = Vector2I.Zero;
        MaxSizeCell = Vector2I.Zero;
    }
}
