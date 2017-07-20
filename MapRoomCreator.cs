using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoomCreator : MonoBehaviour {
    // Id assigned to newly created rooms. Incremented on each assignment
    private int id;

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
    /// Creator of Map rooms in given spawn ellipsis area.
    /// </summary>
    /// <param name="numberToCreate">Number of rooms to create</param>
    /// <param name="spawnEllipsisWidth">Spawn ellipsis area's width</param>
    /// <param name="spawnEllipsisHeight">Spawn ellipsis area's height</param>
    /// <param name="meanHeight">Mean height of rooms to be created</param>
    /// <param name="meanWidth">Mean width of rooms to be created</param>
    /// <param name="standardDeviation">Variance in the creation of room sizes</param>
    /// <param name="maxWidth">Maximum allowable width of created room</param>
    /// <param name="minWidth">Minimum allowable width of created room</param>
    /// <param name="maxHeight">Maximum allowable height of created room</param>
    /// <param name="minHeight">Minimum allowable height of created room</param>
    public MapRoomCreator(int numberToCreate, int spawnEllipsisWidth, int spawnEllipsisHeight,
        int meanHeight, int meanWidth, int standardDeviation, int maxWidth, int minWidth, int maxHeight, int minHeight)
    {
        this.numberToCreate = numberToCreate;

        this.spawnWidth = spawnEllipsisWidth;
        this.spawnHeight = spawnEllipsisHeight;

        this.meanHeight = meanHeight;
        this.meanWidth = meanWidth;

        this.deviation = standardDeviation;

        this.maxWidth = maxWidth;
        this.minWdith = minWidth;

        this.maxHeight = maxHeight;
        this.minHeight = minHeight;
    }

    /// <summary>
    /// Create a number of rooms inside a circle in 2d space
    /// </summary>
    /// <param name="numberOfRooms">Number of rooms to create</param>
    /// <param name="spawnElipsisWidth">Width of the spawning area</param>
    /// <param name="spawnElipsisHeight">Height of the spawning area</param>
    /// <returns></returns>
    public List<MapRoom> CreateRooms(int numberOfRooms, int spawnElipsisWidth, int spawnElipsisHeight)
    {
        // Create rooms inside spawn area
        List<MapRoom> rooms = new List<MapRoom>();
        for (int i = 0; i < numberOfRooms; i++)
        {
            rooms.Add(CreateRoom(spawnElipsisWidth, spawnElipsisHeight));
        }

        return rooms;
    }

    /// <summary>
    /// Create a room inside a given spawn area.
    /// </summary>
    /// <param name="spawnElipsisWidth">Spawn area width</param>
    /// <param name="spawnElipsisHeight">Spawn area height</param>
    /// <returns></returns>
    private MapRoom CreateRoom(int spawnElipsisWidth, int spawnElipsisHeight)
    {
        Vector2 randomPointInElipsis = MathHelpers.RandomPointInElipsis(spawnElipsisWidth, spawnElipsisHeight);

        // Round point to lock to 'grid' space
        Point loc = new Point(Mathf.RoundToInt(randomPointInElipsis.x), Mathf.RoundToInt(randomPointInElipsis.y));

        // Get normal distribution of room sizes
        int width = MathHelpers.FindValueInDistributionRange(meanWidth, deviation, maxWidth, minWdith);
        int height = MathHelpers.FindValueInDistributionRange(meanHeight, deviation, maxHeight, minHeight);

        return new MapRoom(loc, width, height, id++);
    }
}
