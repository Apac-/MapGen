using System.Collections.Generic;

public interface IMapRoomFactory
{
    MapRoom CreateRoom(Point loc, int width, int height);

    List<MapRoom> CreateRooms();

    void UpdateSettings(MapSettings settings);
}