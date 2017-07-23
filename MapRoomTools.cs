using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoomTools {

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
