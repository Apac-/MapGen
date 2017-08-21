using System.Collections.Generic;

namespace MapGen
{
    public interface IMapRoomFactory
    {
        void UpdateSettings(MapSettings settings);

        List<MapRoom> CreateRooms();

        MapRoom CreateRoom(Point loc, int width, int height);

        List<MapRoom> CreateRoomsFromFiller(RoomType[][] map);
    }
}