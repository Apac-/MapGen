using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    public List<MapRoom> fillerRooms { get; private set; }
    public List<Line> hallwayLines { get; private set; }
    public List<MapRoom> hallwayRooms { get; private set; }
    public List<MapRoom> hubRooms { get; private set; }
    public RoomType[][] map { get; private set; }

    public readonly int width;
    public readonly int height;

    public Point greatestPoint { get { return new Point(width, height); } }

    private IMapRoomFactory mapRoomFactory;

    public MapData(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> connectingLines, IMapRoomFactory mapRoomFactory)
    {
        this.hubRooms = hubRooms;
        this.hallwayRooms = hallwayRooms;
        this.hallwayLines = connectingLines;

        this.mapRoomFactory = mapRoomFactory;

        Point bottomLeftPoint = FindBottomLeftPointInMap(hubRooms, hallwayRooms, hallwayLines);
        Point upperRightPoint = FindUpperRightPointInMap(hubRooms, hallwayRooms, hallwayLines);

        hubRooms = OffSetRoomsToZeroOrigin(hubRooms, bottomLeftPoint);
        hallwayRooms = OffSetRoomsToZeroOrigin(hallwayRooms, bottomLeftPoint);
        hallwayLines = OffsetLinesToZeroOrigin(hallwayLines, bottomLeftPoint);

        width = upperRightPoint.X - bottomLeftPoint.X;
        height = upperRightPoint.Y - bottomLeftPoint.Y;
        map = CreateMap(width, height);

        AddRoomsToMap(hubRooms);
        AddRoomsToMap(hallwayRooms);
        AddLinesToMap(hallwayLines);

        fillerRooms = this.mapRoomFactory.CreateRoomsFromFiller(map);
        AddRoomsToMap(fillerRooms);
    }

}