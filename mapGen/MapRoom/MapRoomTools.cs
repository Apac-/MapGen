using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoomTools : IMapRoomTools
{
    public Point MidPointBetweenMapRooms(MapRoom r0, MapRoom r1)
    {
        Point r0c = new Point(Mathf.RoundToInt(r0.centerPoint.x), Mathf.RoundToInt(r0.centerPoint.y));
        Point r1c = new Point(Mathf.RoundToInt(r1.centerPoint.x), Mathf.RoundToInt(r1.centerPoint.y));

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
    public bool IsPointBetweenXBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer)
    {
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
    public bool IsPointBetweenYBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer)
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
    /// Finds rooms within a connected point grouping.
    /// </summary>
    /// <param name="connectedGroup">connected point grouping</param>
    /// <returns></returns>
    public List<HashSet<Point>> FindRoomsInGroup(HashSet<Point> connectedGroup)
    {
        HashSet<Point> unusedPoints = connectedGroup;

        List<HashSet<Point>> foundRooms = new List<HashSet<Point>>();

        // Find all square or rectangle rooms in a grouping of connected points
        while (unusedPoints.Count > 0)
        {
            // Find a room in the unused point group
            HashSet<Point> foundRoom = FindRoomInGroup(unusedPoints);

            // Add found room to the found rooms list
            foundRooms.Add(foundRoom);

            // Remove the points of the found room from the unused Points group
            foreach (Point p in foundRoom)
                unusedPoints.Remove(p);
        }

        return foundRooms;
    }

    /// <summary>
    /// Finds individual rooms within a connected point grouping.
    /// </summary>
    /// <param name="connectedGroup">Avilable points to find room in.</param>
    /// <returns></returns>
    public HashSet<Point> FindRoomInGroup(HashSet<Point> connectedGroup)
    {
        // Init start point to large number, used later to find lower point.
        Point startPoint = new Point(100000, 100000);

        // Find lowest starting point
        foreach (Point p in connectedGroup)
        {
            if (p.X <= startPoint.X && p.Y <= startPoint.Y)
                startPoint = p;
        }

        HashSet<Point> roomPoints = new HashSet<Point>();

        // init helpers
        bool diagUsable = true;
        int diagStep = 0;
        Point lastPoint = startPoint;

        // move up and right one, check
        while (diagUsable)
        {
            Point currentPoint = new Point(startPoint.X + diagStep, startPoint.Y + diagStep);

            if (connectedGroup.Contains(currentPoint))
            {
                List<Point> currentPoints = new List<Point>();
                currentPoints.Add(currentPoint);

                bool downPassed = true;
                bool backPassed = true;

                // check downwards
                for (int i = 1; i <= diagStep; i++)
                {
                    Point stepPoint = new Point(currentPoint.X, currentPoint.Y - i);

                    if (connectedGroup.Contains(stepPoint))
                        currentPoints.Add(stepPoint);
                    else
                        downPassed = false;
                }

                // check backwards
                for (int i = 1; i <= diagStep; i++)
                {
                    Point stepPoint = new Point(currentPoint.X - i, currentPoint.Y);

                    if (connectedGroup.Contains(stepPoint))
                        currentPoints.Add(stepPoint);
                    else
                        backPassed = false;
                }

                // add to list if both pass
                if (downPassed && backPassed)
                {
                    lastPoint = currentPoint;

                    diagStep++;

                    foreach (Point p in currentPoints)
                    {
                        roomPoints.Add(p);
                    }
                }
                else // or flag diag point as not usable
                    diagUsable = false;
            }
            else
                diagUsable = false; // escape while loop

            if (diagUsable == false)
            {
                currentPoint = lastPoint;

                if (diagStep > 0)
                    diagStep--;

                List<Point> currentPoints = new List<Point>();

                // check right
                if (connectedGroup.Contains(new Point(currentPoint.X + 1, currentPoint.Y)))
                {
                    int step = 1;

                    // Move right a step, then check all tiles below it down to start point's y. Repeate till fail.
                    while (true)
                    {
                        List<Point> linePoints = new List<Point>();

                        // Indicates a full slice of tiles from step point to start points y bound.
                        bool fullLineOfPoints = true;

                        // step right
                        Point stepPoint = new Point(currentPoint.X + step, currentPoint.Y);
                        if (connectedGroup.Contains(stepPoint))
                        {
                            linePoints.Add(stepPoint);

                            // check all tiles below step point
                            for (int i = 1; i <= diagStep; i++)
                            {
                                Point p = new Point(stepPoint.X, stepPoint.Y - i);

                                if (connectedGroup.Contains(p))
                                    linePoints.Add(p);
                                else
                                {
                                    fullLineOfPoints = false;
                                    break;
                                }
                            }

                            // If a full slice is found, add to keeper points
                            if (fullLineOfPoints)
                                currentPoints.AddRange(linePoints);
                            else
                                break;
                        }
                        else // Break out if right step is no longer viable
                            break;

                        // Move step one more to right
                        step++;
                    }

                }
                else if (connectedGroup.Contains(new Point(currentPoint.X, currentPoint.Y + 1)))
                { // check up
                    int step = 1;

                    // Move up a step, then check all tiles to the left start point's x. Repeate till fail.
                    while (true)
                    {
                        List<Point> linePoints = new List<Point>();

                        // Indicates if a full slice of tiles from step point to start points y bound.
                        bool fullLineOfPoints = true;

                        // step up 
                        Point stepPoint = new Point(currentPoint.X, currentPoint.Y + step);
                        if (connectedGroup.Contains(stepPoint))
                        {
                            linePoints.Add(stepPoint);

                            // check all tiles to the left of step point
                            for (int i = 1; i <= diagStep; i++)
                            {
                                Point p = new Point(stepPoint.X - i, stepPoint.Y);

                                if (connectedGroup.Contains(p))
                                    linePoints.Add(p);
                                else
                                {
                                    fullLineOfPoints = false;
                                    break;
                                }
                            }

                            // If a full slice is found, add to keeper points
                            if (fullLineOfPoints)
                                currentPoints.AddRange(linePoints);
                            else
                                break;
                        }
                        else // Break out if right step is no longer viable
                            break;

                        // Move step one more
                        step++;
                    }
                }

                // Add found points to final room points
                foreach (Point p in currentPoints)
                {
                    roomPoints.Add(p);
                }
            }
        } // end while

        return roomPoints;
    }

    /// <summary>
    /// Searches given rooms to find one that contains the given point.
    /// Its assumed that rooms do not overlap thus each room contains unique points.
    /// </summary>
    /// <param name="rooms">Map rooms to search through.</param>
    /// <param name="point">Point that should be contained in one of the given rooms.</param>
    /// <returns>Returns null is no rooms found.</returns>
    public MapRoom FindRoomContainingPoint(List<MapRoom> rooms, Vector2 point)
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

    /// <summary>
    /// Finds and returns all rooms that are large enough to be main or 'hub' rooms.
    /// </summary>
    /// <param name="rooms">Rooms to check for hubs</param>
    /// <param name="cutoff">Adjustment to further limit allowed lower size of hub rooms</param>
    /// <returns></returns>
    public List<MapRoom> FindHubRooms(List<MapRoom> rooms, float cutoff)
    {
        int height_mean = 0;
        int width_mean = 0;

        foreach (MapRoom room in rooms)
        {
            height_mean += room.height;
            width_mean += room.width;
        }

        height_mean = Mathf.RoundToInt((float)(height_mean / rooms.Count) * cutoff);
        width_mean = Mathf.RoundToInt((float)(width_mean / rooms.Count) * cutoff);

        List<MapRoom> hubRooms = new List<MapRoom>();
        foreach (MapRoom room in rooms)
        {
            if (room.width > width_mean && room.height > height_mean)
            {
                room.roomType = RoomType.Hub;
                hubRooms.Add(room);
            }
        }

        return hubRooms;
    }

    /// <summary>
    /// Use hallway lines to create raycasts that will be used to find all unused rooms between hubrooms.
    /// </summary>
    /// <param name="hallwayLines">Lines that run from one hub room to another.</param>
    /// <param name="hubRooms">Main 'hub' rooms already discoverd.</param>
    /// <returns></returns>
    public List<MapRoom> FindHallwayRooms(List<Line> hallwayLines, List<MapRoom> hubRooms)
    {
        List<MapRoom> foundRooms = new List<MapRoom>();

        foreach (Line line in hallwayLines)
        {
            // Find heading 
            Vector2 rayHeading = line.p0 - line.p1;

            // Find distance
            float rayDistance = rayHeading.magnitude;

            // Find direction
            Vector2 rayDirection = rayHeading / rayDistance;

            // Throw 2d Raycast
            RaycastHit2D[] roomsHit = Physics2D.RaycastAll(line.p1, rayDirection, rayDistance);

            // Check found rooms vs Hub rooms; Add to hallwayRooms if not a hub room.
            for (int i = 0; i < roomsHit.Length; i++)
            {
                MapRoom room = roomsHit[i].transform.GetComponent<MapRoomHolder>().mapRoom;

                if (!hubRooms.Contains(room) && !foundRooms.Contains(room))
                {
                    room.roomType = RoomType.Hallway;
                    foundRooms.Add(room);
                }
            }
        }

        return foundRooms;
    }
}