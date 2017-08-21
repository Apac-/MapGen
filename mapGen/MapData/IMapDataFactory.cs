using System.Collections.Generic;

namespace MapGen
{
    public interface IMapDataFactory
    {
        RoomType[][] AddLinesToMap(List<Line> lines, RoomType[][] map);
        RoomType[][] AddRoomsToMap(List<MapRoom> rooms, RoomType[][] map);
        RoomType[][] CreateMap(int mapWidth, int mapHeight);
        MapData CreateNewMapData(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines, IMapRoomFactory mapRoomFactory);
        Point FindBottomLeftPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines);
        Point FindGreatestPointInHallways(List<Line> lines);
        Point FindGreatestPointInRooms(List<MapRoom> rooms);
        Point FindLowestPointInHallways(List<Line> lines);
        Point FindLowestPointInRooms(List<MapRoom> rooms);
        Point FindUpperRightPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines);
        List<Line> OffsetLinesToZeroOrigin(List<Line> lines, Point offsetPoint);
        List<MapRoom> OffSetRoomsToZeroOrigin(List<MapRoom> rooms, Point offsetPoint);
    }
}