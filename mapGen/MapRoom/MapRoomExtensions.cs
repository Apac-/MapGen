using System.Collections.Generic;
using UnityEngine;

public static class MapRoomExtensions
{

    /// <summary>
    /// Returns true if given point is within room.
    /// </summary>
    /// <param name="p">The given point to find</param>
    /// <returns></returns>
    public static bool ContainsPoint(this MapRoom room, Point p)
    {

        if (p.X >= room.gridLocation.X &&
            p.X <= room.endPoint.X &&
            p.Y >= room.gridLocation.Y &&
            p.Y <= room.endPoint.Y)
        {
            return true;
        }
        else
            return false;
    }

    public static bool ContainsPoint(this MapRoom room, Vector2 p)
    {
        Point point = p;
        return room.ContainsPoint(point);
    }

}