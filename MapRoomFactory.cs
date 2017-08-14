using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoomFactory : IMapRoomFactory
{
    // Id assigned to newly created rooms. Incremented on each assignment
    private int _id = 0;
    private int id { get { return _id++; } }

    private int numberToCreate;

    private int maxHeight;
    private int maxWidth;

    private int meanHeight;
    private int meanWidth;

    private int minHeight;
    private int minWdith;

    private int deviation;

    private int spawnHeight;
    private int spawnWidth;

    private IMapRoomTools mapRoomTools;

    public MapRoomFactory(IMapRoomTools mapRoomTools)
    {
        this.mapRoomTools = mapRoomTools;
    }

    /// <summary>
    /// Sets or updates the room creator with new settings.
    /// Needs to be called before any creation calls.
    /// </summary>
    /// <param name="settings"></param>
    public void UpdateSettings(MapSettings settings)
    {
        _id = 0;

        numberToCreate = settings.numberOfRoomsToCreate;

        spawnHeight = settings.roomSpawnEllipsisAreaHeight;
        spawnWidth = settings.roomSpawnEllipsisAreaWidth;

        meanHeight = settings.roomMeanHeight;
        meanWidth = settings.roomMeanWidth;

        deviation = settings.roomStandardDeviation;

        maxHeight = settings.roomMaxHeight;
        maxWidth = settings.roomMaxWidth;

        minHeight = settings.roomMinHeight;
        minWdith = settings.roomMinWidth;
    }

    /// <summary>
    /// Create a number of rooms inside a circle in 2d space
    /// </summary>
    /// <returns></returns>
    public List<MapRoom> CreateRooms()
    {
        // Create rooms inside spawn area
        List<MapRoom> rooms = new List<MapRoom>();
        for (int i = 0; i < numberToCreate; i++)
        {
            rooms.Add(CreateRoomInSpawnArea());
        }

        return rooms;
    }

    /// <summary>
    /// Create a room inside a given spawn area.
    /// </summary>
    /// <returns></returns>
    private MapRoom CreateRoomInSpawnArea()
    {
        Vector2 randomPointInElipsis = MathHelpers.RandomPointInElipsis(spawnWidth, spawnHeight);

        // Round point to lock to 'grid' space
        Point loc = new Point(Mathf.RoundToInt(randomPointInElipsis.x), Mathf.RoundToInt(randomPointInElipsis.y));

        // Get normal distribution of room sizes
        int width = MathHelpers.FindValueInDistributionRange(meanWidth, deviation, maxWidth, minWdith);
        int height = MathHelpers.FindValueInDistributionRange(meanHeight, deviation, maxHeight, minHeight);

        return new MapRoom(loc, width, height, id);
    }

    /// <summary>
    /// Creates a map room at specified point.
    /// </summary>
    /// <param name="loc">Start location</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>Newly created maproom at point with id</returns>
    public MapRoom CreateRoom(Point loc, int width, int height)
    {
        return new MapRoom(loc, width, height, id);
    }

    /// <summary>
    /// Creates map rooms from filler space in a basic map.
    /// </summary>
    /// <param name="map">Basic map 2d array</param>
    /// <returns></returns>
    public List<MapRoom> CreateRoomsFromFiller(RoomType[][] map)
    {
        // Look for all 'tiles' that are ID of 1 (filler).
        HashSet<Point> fillerPoints = new HashSet<Point>();
        for (int x = 0; x < map.Length; x++)
        {
            for (int y = 0; y < map[x].Length; y++)
            {
                if (map[x][y] == RoomType.Filler)
                    fillerPoints.Add(new Point(x, y));
            }
        }

        // use Flood fill to find groupings of connected points on grid space
        List<HashSet<Point>> fillerGroupings = new List<HashSet<Point>>();
        HashSet<Point> usedPoints = new HashSet<Point>();
        foreach (Point p in fillerPoints) {
            if (usedPoints.Contains(p))
                continue;

            HashSet<Point> room = new HashSet<Point>();
            room.Add(p);
            usedPoints.Add(p);

            Queue<Point> q = new Queue<Point>();
            q.Enqueue(p);

            while (q.Count > 0)
            {
                Point current = q.Dequeue();

                List<Point> adjacentPoints = GetSurroundingPoints(current, fillerPoints);

                foreach (Point adjacentPoint in adjacentPoints)
                {
                    if (!usedPoints.Contains(adjacentPoint))
                    {
                        usedPoints.Add(adjacentPoint);

                        room.Add(adjacentPoint);

                        q.Enqueue(adjacentPoint);
                    }
                }
            }

            fillerGroupings.Add(room);
        }

        List<MapRoom> rooms = new List<MapRoom>();

        // Take in rooms List<HashSet<Point>> and create map rooms out of it.
        foreach (HashSet<Point> fillerGroup in fillerGroupings) {
            List<HashSet<Point>> seperatedRooms = mapRoomTools.FindRoomsInGroup(fillerGroup);

            // Create a collection of dungeon rooms from hashSets of points
            rooms.AddRange(CreateRoomsFromHashSets(seperatedRooms));
        }

        foreach (MapRoom room in rooms)
        {
            room.roomType = RoomType.Filler;
        }

        return rooms;
    }

    /// <summary>
    /// Creates MapRooms from hashsets of points that are already processed into squares or rectangles.
    /// </summary>
    /// <param name="rooms">Pre-processed sets of points that make up square or rectangle room groupings</param>
    /// <returns></returns>
    private List<MapRoom> CreateRoomsFromHashSets(List<HashSet<Point>> rooms)
    {
        List<MapRoom> createdRooms = new List<MapRoom>();

        foreach (HashSet<Point> room in rooms)
        {
            int width = 1;
            int height = 1;

            Point startPoint = new Point(100000, 100000);

            foreach (Point p in room)
            {
                // Find lowest starting point
                if (p.X <= startPoint.X && p.Y <= startPoint.Y)
                    startPoint = p;

                // get width
                int tempWidth = p.X - startPoint.X + 1;
                if (tempWidth > width)
                    width = tempWidth;

                // get height
                int tempHeight = p.Y - startPoint.Y + 1;
                if (tempHeight > height)
                    height = tempHeight;
            }
            createdRooms.Add(CreateRoom(startPoint, width, height));
        }

        return createdRooms;
    }

    /// <summary>
    /// Find points surrounding target point in avilable map space.
    /// </summary>
    /// <param name="targetPoint">Point in grid space to check surrounding area of</param>
    /// <param name="avilablePoints">Points that are on map</param>
    /// <returns></returns>
    private List<Point> GetSurroundingPoints(Point targetPoint, HashSet<Point> avilablePoints)
    {
        List<Point> points = new List<Point>();

        // -1,1. 0,1. 1,1
        // -1,0. 0,0. 1,0
        // -1,-1. 0,-1. 1,-1
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                // If target point (center) then skip
                if (yOffset == 0 && xOffset == 0)
                    continue;

                Point p = new Point(targetPoint.X + xOffset, targetPoint.Y + yOffset);

                if (avilablePoints.Contains(p))
                    // If the point is a corner, make sure its reachable from center. No corner cutting
                    if (xOffset == -1 && yOffset == -1
                        || xOffset == -1 && yOffset == 1
                        || xOffset == 1 && yOffset == -1
                        || xOffset == 1 && yOffset == 1)
                    {
                        // Skip if this is a corner point
                        continue;
                    }
                    else
                        points.Add(p);
            }
        }

        return points;
    }
}
