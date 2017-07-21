using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoom {
    public int Id { get; private set; }

    // Location is bottom left of the room
    public Point gridLocation { get; set; }

    public Vector2 centerPoint { get { return new Vector2(gridLocation.X + (width / 2), gridLocation.Y + (height / 2)); } }

    public Point endPoint { get { return new Point(gridLocation.X + width, gridLocation.Y + height); } }

    public int width { get; private set; }
    public int height { get; private set; }

    public MapRoom(Point location, int width, int height, int id)
    {
        Id = id;
        this.width = width;
        this.height = height;
        gridLocation = location;
    }

}
