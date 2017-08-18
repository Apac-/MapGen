using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IMapDataFactory
{
    MapData CreateNewMapData(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines, IMapRoomFactory mapRoomFactory);
}
