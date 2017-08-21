using System;
using System.Collections;
using System.Collections.Generic;

namespace MapGen
{
    public class MapData
    {
        public RoomType[][] map { get; private set; }

        public List<MapRoom> fillerRooms { get; private set; }
        public List<MapRoom> hallwayRooms { get; private set; }
        public List<MapRoom> hubRooms { get; private set; }

        public List<Line> hallwayLines { get; private set; }

        public Point greatestPoint { get { return new Point(width, height); } }

        public readonly int width;
        public readonly int height;

        public MapData(RoomType[][] map,
                       List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<MapRoom> fillerRooms,
                       List<Line> hallwayLines, int width, int height)
        {
            this.map = map;

            this.hubRooms = hubRooms;
            this.hallwayRooms = hallwayRooms;
            this.fillerRooms = fillerRooms;

            this.hallwayLines = hallwayLines;

            this.width = width;
            this.height = height;
        }

    }
}