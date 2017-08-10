using System.Collections.Generic;
using UnityEngine;

public interface IMapRoomTools
{
    List<MapRoom> CreateRoomsFromFiller(RoomType[][] map, IMapRoomFactory roomFactory);

    List<MapRoom> FindHallwayRooms(List<Line> hallwayLines, List<MapRoom> hubRooms);

    List<MapRoom> FindHubRooms(List<MapRoom> rooms, float cutoff);

    Point FindLowestPointInRooms(List<MapRoom> rooms);

    MapRoom FindRoomContainingPoint(List<MapRoom> rooms, Vector2 point);

    bool IsPointBetweenXBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer);

    bool IsPointBetweenYBoundariesOfGivenRooms(Point midPoint, MapRoom r0, MapRoom r1, int buffer);

    Point MidPointBetweenMapRooms(MapRoom r0, MapRoom r1);
}