using System.Collections.Generic;
using UnityEngine;

namespace MapGen
{
    public interface IMapRoomTools
    {
        List<MapRoom> FindHallwayRooms(List<Line> hallwayLines, List<MapRoom> hubRooms);

        List<MapRoom> FindHubRooms(List<MapRoom> rooms, float cutoff);

        MapRoom FindRoomContainingPoint(List<MapRoom> rooms, Vector2 point);

        bool IsPointBetweenXBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer);
        bool IsPointBetweenYBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer);

        Point MidPointBetweenMapRooms(MapRoom r0, MapRoom r1);

        List<HashSet<Point>> FindRoomsInGroup(HashSet<Point> connectedGroup);

        HashSet<Point> FindRoomInGroup(HashSet<Point> connectedGroup);
    }
}