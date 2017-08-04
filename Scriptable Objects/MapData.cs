using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData 
{
    public List<MapRoom> fillerRooms { get; private set; }
    public List<Line> hallwayLines { get; private set; }
    public List<MapRoom> hallwayRooms { get; private set; }
    public List<MapRoom> hubRooms { get; private set; }
    public Point greatestPoint { get; private set; }
    public int[][] map { get; private set; }

    public MapData(int[][] map, List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<MapRoom> fillerRooms, List<Line> hallwayLines, Point greatestPoint)
    {
        this.map = map;
        this.hubRooms = hubRooms;
        this.hallwayRooms = hallwayRooms;
        this.fillerRooms = fillerRooms;
        this.hallwayLines = hallwayLines;
        this.greatestPoint = greatestPoint;
    }
}
