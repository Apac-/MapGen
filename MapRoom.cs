using System.Collections;
using System.Collections.Generic;

public class MapRoom {
    public int Id { get; private set; }

    // Location is bottom left of the room
    public Point GridLocation { get; private set; }

    public Point CenterPoint { get { return new Point(GridLocation.X + Width / 2, GridLocation.Y + Height / 2); } }
    public Point EndPoint { get { return new Point(GridLocation.X + Width, GridLocation.Y + Height); } }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapRoom(Point location, int width, int height, int id)
    {
        Id = id;
        Width = width;
        Height = height;
        GridLocation = location;
    }
}
