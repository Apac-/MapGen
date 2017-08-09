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

    /// <summary>
    /// Sets or updates the room creator with new settings.
    /// Needs to be called before any creation calls.
    /// </summary>
    /// <param name="settings"></param>
    public void UpdateSettings(MapSettings settings)
    {
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
}
