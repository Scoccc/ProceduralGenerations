using System.Collections.Generic;
using System;

public static class Extensions
{
    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Side Opposite(this Side side)
    {
        switch (side)
        {
            case Side.Right: return Side.Left;
            case Side.Left: return Side.Right;
            case Side.Top: return Side.Bottom;
            case Side.Bottom: return Side.Top;
        }
        return Side.Right; // Fallback, though this line should not be reached.
    }

    /// <summary>
    /// Returns a random element from the list.
    /// Throws an exception if the list is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    /// <param name="list">The list from which to get a random element.</param>
    /// <returns>A randomly selected element of type T.</returns>
    public static T GetRandomElement<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("Empty or null list");

        int index = rng.Next(list.Count);
        return list[index];
    }
}

/// <summary>
/// Represents the four cardinal directions used to indicate neighbor relations in the maze.
/// </summary>
public enum Side
{
    Right,
    Bottom,
    Left,
    Top
}
