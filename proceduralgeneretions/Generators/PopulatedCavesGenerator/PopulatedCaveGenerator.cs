using Godot;
using System.Collections.Generic;
[GlobalClass]
public partial class PopulatedCaveGenerator : Generator<PopulatedCave>
{
    [Export] public CaveGenerator caveGenerator { get; set; }
    [Export] public RoomGenerator roomGenerator { get; set; }
    [Export] public Vector2I minSize;
    [Export(PropertyHint.Range, "0,0,or_greater")] public int maxNumberOfGroups = 1;
    [Export(PropertyHint.Range, "0,0,or_greater")] public int maxGroupSpread = 10;
    [Export(PropertyHint.Range, "0,0,or_greater")] public int roomsMinDinstance = 1;
    private List<Rect2I> spacesFound = new List<Rect2I>();
    private Cave cave;
    private List<Room> rooms = new List<Room>();
    public List<List<Rect2I>> groups = new List<List<Rect2I>>();

    public override PopulatedCave generate()
    {
        caveGenerator.minThreshold = 1f;
        cave = caveGenerator.generate()[0];

        FindFreeSpaces(cave, minSize);
        groups = FindConnectedGroups();
        groups.Sort((a, b) => b.Count - a.Count);

        if (maxNumberOfGroups < groups.Count)
            groups.RemoveRange(maxNumberOfGroups, groups.Count - maxNumberOfGroups);

        foreach (List<Rect2I> group in groups)
        {
            if (group.Count > maxGroupSpread)
            {
                group.RemoveRange(maxGroupSpread, group.Count - maxGroupSpread);
            }
        }

        PlaceRooms();

        return new PopulatedCave(cave, rooms);
    }

    public void FindFreeSpaces(Cave cave, Vector2I size)
    {
        bool[,] validArea = new bool[cave.GetSize().X, cave.GetSize().Y];

        foreach (Vector2I cell in cave.PathTiles)
        {
            validArea[cell.X - (int)cave.GetGlobalPosition().X, cell.Y - (int)cave.GetGlobalPosition().Y] = true;
        }

        for (int x = 0; x < validArea.GetLength(0); x++)
        {
            for (int y = 0; y < validArea.GetLength(1); y++)
            {
                if (validArea[x, y])
                {
                    Rect2I rect = new Rect2I(new Vector2I(x, y), size);
                    if (CheckSpace(rect, validArea))
                    {
                        spacesFound.Add(new Rect2I(rect.Position.X + (int)cave.GetGlobalPosition().X,
                                                   rect.Position.Y + (int)cave.GetGlobalPosition().Y,
                                                   rect.Size));
                    }
                }
            }
        }
    }

    public List<List<Rect2I>> FindConnectedGroups()
    {
        List<List<Rect2I>> groups = new List<List<Rect2I>>();
        List<Rect2I> remaining = new List<Rect2I>(spacesFound);

        while (remaining.Count > 0)
        {
            List<Rect2I> group = new List<Rect2I>();
            Queue<Rect2I> queue = new Queue<Rect2I>();
            queue.Enqueue(remaining[0]);
            group.Add(remaining[0]);
            remaining.RemoveAt(0);

            while (queue.Count > 0)
            {
                Rect2I current = queue.Dequeue();
                List<Rect2I> connected = remaining.FindAll(rect => rect.Intersects(current));

                foreach (var rect in connected)
                {
                    queue.Enqueue(rect);
                    group.Add(rect);
                    remaining.Remove(rect);
                }
            }
            groups.Add(group);
        }
        return groups;
    }

    public bool CheckSpace(Rect2I space, bool[,] validArea)
    {
        bool result = true;

        if (space.Position.X + space.Size.X > validArea.GetLength(0) || space.Position.Y + space.Size.Y > validArea.GetLength(1))
            return false;

        for (int x = 0; x < space.Size.X; x++)
        {
            for (int y = 0; y < space.Size.Y; y++)
            {
                result &= validArea[space.Position.X + x, space.Position.Y + y];
            }
        }

        return result;
    }

    public void PlaceRooms()
    {
        foreach (List<Rect2I> group in groups)
        {
            List<Rect2I> _temp = new List<Rect2I>(group);
            do
            {
                _temp.Shuffle();
                Rect2I current = _temp[0];
                roomGenerator.MaxFreeSpace = current;
                roomGenerator.AttachTo = RoomGenerator.Position.Centered;
                Room newRoom = roomGenerator.generate();
                if (newRoom != null)
                {
                    rooms.Add(newRoom);
                    _temp.RemoveAll(space => space.Intersects(newRoom.GetArea().Grow(roomsMinDinstance)));
                }
                else
                {
                    _temp.RemoveAll(space => space.Intersects(current));
                }

                _temp.Remove(current);
            }
            while (_temp.Count > 0);
        }
    }
}
