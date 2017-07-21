using System;
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

    public static Point MidPointBetweenMapRooms(MapRoom r0, MapRoom r1)
    {
        Point r0c = r0.centerPoint;
        Point r1c = r1.centerPoint;

        return new Point((r0c.X + r1c.X) / 2, (r0c.Y + r1c.Y) / 2);
    }

    /// <summary>
    /// Checks if the mid point between these rooms is within the X plane boundaries of both rooms.
    /// </summary>
    /// <param name="midPoint">Point between rooms</param>
    /// <param name="r0">First room</param>
    /// <param name="r1">Second room</param>
    /// <param name="buffer">Makes sure the point isn't directly on a room boundary</param>
    /// <returns></returns>
    public static bool IsPointBetweenXBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer) {
        if (midPoint.X >= r0.gridLocation.X + buffer && midPoint.X <= r0.endPoint.X - buffer)
        {
            if (midPoint.X >= r1.gridLocation.X + buffer && midPoint.X <= r1.endPoint.X - buffer)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the mid point between these rooms is within the Y plane boundaries of both rooms.
    /// </summary>
    /// <param name="midPoint">Point between rooms</param>
    /// <param name="r0">First room</param>
    /// <param name="r1">Second room</param>
    /// <param name="buffer">Makes sure the point isn't directly on a room boundary</param>
    /// <returns></returns>
    public static bool IsPointBetweenYBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer)
    {
        if (midPoint.Y >= r0.gridLocation.Y + buffer && midPoint.Y <= r0.endPoint.Y - buffer)
        {
            if (midPoint.Y >= r1.gridLocation.Y + buffer && midPoint.Y <= r1.endPoint.Y - buffer)
            {
                return true;
            }
        }

        return false;
    }
}
