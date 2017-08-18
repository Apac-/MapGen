using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MapDataFactory : IMapDataFactory
{
    public MapData CreateNewMapData(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines, IMapRoomFactory mapRoomFactory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Changes lines to be based on a map that has a 0,0 origin.
    /// </summary>
    /// <param name="lines">The lines to offset.</param>
    /// <param name="offsetPoint">Lowest point in world location on map.</param>
    /// <returns></returns>
    public List<Line> OffsetLinesToZeroOrigin(List<Line> lines, Point offsetPoint)
    {
        List<Line> offsetLines = new List<Line>();

        foreach (Line line in lines)
        {
            // Snap line locations to grid space (ints)
            int p0x = Mathf.RoundToInt(line.p0.x) - offsetPoint.X;
            int p0y = Mathf.RoundToInt(line.p0.y) - offsetPoint.Y;

            int p1x = Mathf.RoundToInt(line.p1.x) - offsetPoint.X;
            int p1y = Mathf.RoundToInt(line.p1.y) - offsetPoint.Y;

            offsetLines.Add(new Line(new Vector2(p0x, p0y), new Vector2(p1x, p1y)));
        }

        return offsetLines;
    }

    /// <summary>
    /// Changes rooms to be based on a map that has a 0,0 origin.
    /// </summary>
    /// <param name="rooms">Rooms to change</param>
    /// <param name="offsetPoint">The lowest point in world location on map.</param>
    public List<MapRoom> OffSetRoomsToZeroOrigin(List<MapRoom> rooms, Point offsetPoint)
    {
        foreach (MapRoom room in rooms)
        {
            Point newGridLocation = new Point(room.gridLocation.X - offsetPoint.X, room.gridLocation.Y - offsetPoint.Y);
            room.gridLocation = newGridLocation;
        }

        return rooms;
    }

    /// <summary>
    /// Modifies the basic map array with filler ids (1) in line positions.
    /// </summary>
    /// <param name="map">Basic map array</param>
    /// <param name="lines">Lines that will be added to map as filler</param>
    public void AddLinesToMap(List<Line> lines)
    {
        foreach (Line line in lines)
        {
            Point p0 = new Point((int)line.p0.x, (int)line.p0.y);
            Point p1 = new Point((int)line.p1.x, (int)line.p1.y);

            // Determine if the width and height steps will go in negitive directions.
            int negX = p0.X < p1.X ? 1 : -1;
            int negY = p0.Y < p1.Y ? 1 : -1;

            int absWidth = Mathf.Abs(p0.X - p1.X);
            int absHeight = Mathf.Abs(p0.Y - p1.Y);

            // Set the map area to 1, which is the 'filler' UID. 
            for (int x = 0; x <= absWidth; x++)
            {
                // Get the width position.
                int xPos = p0.X + (x * negX);

                for (int y = 0; y <= absHeight; y++)
                {
                    // Get the height position
                    int yPos = p0.Y + (y * negY);

                    // If the map position is not already assigned, then add 'filler' for hallway. Set to 1. 
                    if (map[xPos][yPos] == RoomType.Empty)
                        map[xPos][yPos] = RoomType.Filler;
                }
            }
        }
    }

    /// <summary>
    /// Modfies the basic map array with room ids in their positions.
    /// </summary>
    /// <param name="map">Basic map array</param>
    /// <param name="rooms">Rooms to add to map</param>
    public void AddRoomsToMap(List<MapRoom> rooms)
    {
        foreach (MapRoom room in rooms)
        {
            int locX = room.gridLocation.X;
            int locY = room.gridLocation.Y;

            for (int x = 0; x < room.width; x++)
            {
                for (int y = 0; y < room.height; y++)
                {
                    map[locX + x][locY + y] = room.roomType;
                }
            }
        }
    }

    /// <summary>
    /// Create and zero out a 2d array for the basic map info.
    /// </summary>
    /// <param name="mapWidth">Width of the used map area.</param>
    /// <param name="mapHeight">Height of the used map area.</param>
    /// <returns></returns>
    public RoomType[][] CreateMap(int mapWidth, int mapHeight)
    {
        RoomType[][] map = new RoomType[mapWidth][];

        // Zero map out
        for (int mapX = 0; mapX < map.Length; mapX++)
        {
            map[mapX] = new RoomType[mapHeight];

            for (int mapY = 0; mapY < map[mapX].Length; mapY++)
            {
                map[mapX][mapY] = RoomType.Empty;
            }
        }

        return map;
    }

    /// <summary>
    /// Finds the upper right used point in given rooms and lines.
    /// </summary>
    /// <param name="hubRooms"></param>
    /// <param name="hallwayRooms"></param>
    /// <param name="hallwayLines"></param>
    /// <returns></returns>
    public Point FindUpperRightPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines)
    {
        List<Point> points = new List<Point>()
        {
            FindGreatestPointInRooms(hubRooms),
            FindGreatestPointInRooms(hallwayRooms),
            FindGreatestPointInHallways(hallwayLines),
        };

        int greatestX = 0;
        int greatestY = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (i == 0)
            {
                greatestX = points[i].X;
                greatestY = points[i].Y;
            }
            else
            {
                if (greatestX < points[i].X)
                    greatestX = points[i].X;
                if (greatestY < points[i].Y)
                    greatestY = points[i].Y;
            }
        }

        return new Point(greatestX, greatestY);
    }

    /// <summary>
    /// Finds the lower left used point in the given rooms and lines.
    /// </summary>
    /// <param name="hubRooms"></param>
    /// <param name="hallwayRooms"></param>
    /// <param name="hallwayLines"></param>
    /// <returns></returns>
    public Point FindBottomLeftPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines)
    {
        List<Point> points = new List<Point>()
        {
            FindLowestPointInRooms(hubRooms),
            FindLowestPointInRooms(hallwayRooms),
            FindLowestPointInHallways(hallwayLines),
        };

        int lowestX = 0;
        int lowestY = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (i == 0)
            {
                lowestX = points[i].X;
                lowestY = points[i].Y;
            }
            else
            {
                if (lowestX > points[i].X)
                    lowestX = points[i].X;
                if (lowestY > points[i].Y)
                    lowestY = points[i].Y;
            }
        }

        return new Point(lowestX, lowestY);
    }

    /// <summary>
    /// Finds the point at the upper right of the map.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public Point FindGreatestPointInHallways(List<Line> lines)
    {
        int greatestX = 0;
        int greatestY = 0;

        int roundedX = 0;
        int roundedY = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (i == 0)
            {
                greatestX = Mathf.RoundToInt(lines[i].p0.x);
                greatestY = Mathf.RoundToInt(lines[i].p0.y);

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;
            }
            else
            {
                roundedX = Mathf.RoundToInt(lines[i].p0.x);
                roundedY = Mathf.RoundToInt(lines[i].p0.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (greatestX < roundedX)
                    greatestX = roundedX;
                if (greatestY < roundedY)
                    greatestY = roundedY;
            }
        }

        return new Point(greatestX, greatestY);
    }

    /// <summary>
    /// Finds the point at the lower left of the map.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    public Point FindLowestPointInHallways(List<Line> lines)
    {
        int lowestX = 0;
        int lowestY = 0;

        int roundedX = 0;
        int roundedY = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (i == 0)
            {
                lowestX = Mathf.RoundToInt(lines[i].p0.x);
                lowestY = Mathf.RoundToInt(lines[i].p0.y);

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;
            }
            else
            {
                roundedX = Mathf.RoundToInt(lines[i].p0.x);
                roundedY = Mathf.RoundToInt(lines[i].p0.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;

                roundedX = Mathf.RoundToInt(lines[i].p1.x);
                roundedY = Mathf.RoundToInt(lines[i].p1.y);

                if (lowestX > roundedX)
                    lowestX = roundedX;
                if (lowestY > roundedY)
                    lowestY = roundedY;
            }
        }

        return new Point(lowestX, lowestY);
    }

    /// <summary>
    /// Finds the point at the upper right of the map.
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    public Point FindGreatestPointInRooms(List<MapRoom> rooms)
    {
        int greatestX = 0;
        int greatestY = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0)
            {
                greatestX = rooms[i].endPoint.X;
                greatestY = rooms[i].endPoint.Y;
            }
            else
            {
                if (greatestX < rooms[i].endPoint.X)
                    greatestX = rooms[i].endPoint.X;
                if (greatestY < rooms[i].endPoint.Y)
                    greatestY = rooms[i].endPoint.Y;
            }
        }

        return new Point(greatestX, greatestY);
    }

    /// <summary>
    /// Finds the used point in the bottom left of the map.
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    public Point FindLowestPointInRooms(List<MapRoom> rooms)
    {
        int lowestX = 0;
        int lowestY = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0)
            {
                lowestX = rooms[i].gridLocation.X;
                lowestY = rooms[i].gridLocation.Y;
            }
            else
            {
                if (lowestX > rooms[i].gridLocation.X)
                    lowestX = rooms[i].gridLocation.X;
                if (lowestY > rooms[i].gridLocation.Y)
                    lowestY = rooms[i].gridLocation.Y;
            }
        }

        return new Point(lowestX, lowestY);
    }

}
