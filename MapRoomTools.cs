using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapRoomTools {

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


    /// <summary>
    /// Finds the point at the upper right of the map.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    internal static Point FindGreatestPointInHallways(List<Line> lines)
    {
        int greatestX = 0;
        int greatestY = 0;

        int roundedX = 0;
        int roundedY = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (i == 0)
            {
                greatestX = Mathf.RoundToInt(lines[i].p0.x);
                greatestY = Mathf.RoundToInt(lines[i].p0.y);

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;
            }
            else
            {
                roundedX = Mathf.RoundToInt(lines[i].p0.x);
                roundedY = Mathf.RoundToInt(lines[i].p0.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;
            }
        }

        return new Point(greatestX, greatestY);
    }

    /// <summary>
    /// Finds the point at the lower left of the map.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    internal static Point FindLowestPointInHallways(List<Line> lines)
    {
        int lowestX = 0;
        int lowestY = 0;

        int roundedX = 0;
        int roundedY = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (i == 0)
            {
                lowestX = Mathf.RoundToInt(lines[i].p0.x);
                lowestY = Mathf.RoundToInt(lines[i].p0.y);

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;
            }
            else
            {
                roundedX = Mathf.RoundToInt(lines[i].p0.x);
                roundedY = Mathf.RoundToInt(lines[i].p0.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;
            }
        }

        return new Point(lowestX, lowestY);
    }

    /// <summary>
    /// Finds the point at the upper right of the map.
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    internal static Point FindGreatestPointInRooms(List<MapRoom> rooms)
    {
        int greatestX = 0;
        int greatestY = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0)
            {
                greatestX = rooms[i].endPoint.X;
                greatestY = rooms[i].endPoint.Y;
            }
            else
            {
                if (greatestX < rooms[i].endPoint.X)
                    greatestX = rooms[i].endPoint.X;
                if (greatestY < rooms[i].endPoint.Y)
                    greatestY = rooms[i].endPoint.Y;
            }
        }

        return new Point(greatestX, greatestY);
    }

    /// <summary>
    /// Finds the used point in the bottom left of the map.
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    public static Point FindLowestPointInRooms(List<MapRoom> rooms)
    {
        int lowestX = 0;
        int lowestY = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0)
            {
                lowestX = rooms[i].gridLocation.X;
                lowestY = rooms[i].gridLocation.Y;
            }
            else
            {
                if (lowestX > rooms[i].gridLocation.X)
                    lowestX = rooms[i].gridLocation.X;
                if (lowestY > rooms[i].gridLocation.Y)
                    lowestY = rooms[i].gridLocation.Y;
            }
        }

        return new Point(lowestX, lowestY);
    }

    /// <summary>
    /// Searches given rooms to find one that contains the given point.
    /// Its assumed that rooms do not overlap thus each room contains unique points.
    /// </summary>
    /// <param name="rooms">Map rooms to search through.</param>
    /// <param name="point">Point that should be contained in one of the given rooms.</param>
    /// <returns>Returns null is no rooms found.</returns>
    public static MapRoom FindRoomContainingPoint(List<MapRoom> rooms, Vector2 point)
    {
        foreach (MapRoom room in rooms)
        {
            if (room.ContainsPoint(point))
            {
                return room;
            }
        }

        return null;
    }
}
