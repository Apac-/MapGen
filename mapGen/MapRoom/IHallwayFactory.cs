using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IHallwayFactory
{
    List<Line> CreateHallwayLinesFromSegments(List<Line> connectingLineSegments, List<MapRoom> rooms, int sizeOfHallways, IMapRoomTools mapRoomTools);
}