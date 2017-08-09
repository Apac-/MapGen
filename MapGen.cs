using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour {
    public MapSettings mapSettings;

    public GameObject physicalRoom;

    private enum GenerationState { Waiting, RoomsSeperated, Reset, Finished }
    private GenerationState currentState;

    private List<MapRoom> mapRooms;

    private IMapRoomFactory mapRoomFactory;

    private MapGenVisualDebugger visualDebugger;

    // Use this for initialization
    void Start () {
        currentState = GenerationState.Waiting;
        visualDebugger = gameObject.GetComponent<MapGenVisualDebugger>();
	}
	
	// Update is called once per frame
	void Update () {
        switch (currentState)
        {
            case GenerationState.Waiting:
                break;
            case GenerationState.RoomsSeperated:
                MapData mapData = GenerateMap(mapRooms);
                if (mapData != null)
                    visualDebugger.SetMapData(mapData);
                break;
            case GenerationState.Reset:
                ResetAndRegenerate();
                break;
            case GenerationState.Finished:
                ResetGeneration();
                currentState = GenerationState.Waiting;
                break;
        }
    }

    /// <summary>
    /// Generate the foundational rooms and objects then wait for physical rooms to seperate to continue.
    /// </summary>
    public void Generate()
    {
        // Start fresh
        ResetGeneration();

        // State is waiting for coroutine to finish
        currentState = GenerationState.Waiting;

        mapRooms = mapRoomFactory.CreateRooms();

        GeneratePhysicalRooms(mapRooms);

        StartCoroutine(WaitTillRoomsSeperate(this.transform));
    }

    /// <summary>
    /// Used after physical rooms have seperated (touching but not overlapping) to create map data from their locations.
    /// </summary>
    /// <param name="rooms">Rooms that have been seperated (not overlapping)</param>
    /// <returns></returns>
    private MapData GenerateMap(List<MapRoom> rooms)
    {
        SnapRoomLocationToGrid(this.transform);

        List<MapRoom> hubRooms = MapRoomTools.FindHubRooms(rooms, mapSettings.hubRoomCutoff);

        // Re-generate if not enough hub rooms are found
        if (hubRooms.Count <= mapSettings.minAmountOfHubRooms)
        {
            currentState = GenerationState.Reset;
            return null;
        }

        List<Vector2> hubRoomCenterPoints = new List<Vector2>();
        foreach (MapRoom room in hubRooms)
        {
            hubRoomCenterPoints.Add(room.centerPoint);
        }

        List<LineSegment> connectingLineSegments = DelaunayGrapher.FindConnectingLineSegments(hubRoomCenterPoints, mapSettings.percentOfRoomConnectionAboveMinPath);

        List<Line> hallwayLines = CreateHallwayLinesFromSegments(connectingLineSegments, hubRooms);

        List<MapRoom> hallwayRooms = MapRoomTools.FindHallwayRooms(hallwayLines, hubRooms);

        Point bottomLeftPoint = FindBottomLeftPointInMap(hubRooms, hallwayRooms, hallwayLines);
        Point upperRightPoint = FindUpperRightPointInMap(hubRooms, hallwayRooms, hallwayLines);
        Debug.Log("Bottom left: " + bottomLeftPoint + " || Up right: " + upperRightPoint);


        hubRooms.TranslateWorldToGridLocation(bottomLeftPoint);
        hallwayRooms.TranslateWorldToGridLocation(bottomLeftPoint);
        hallwayLines = LineTools.TranslateWorldToGridLocation(hallwayLines, bottomLeftPoint);

        int mapWidth = upperRightPoint.X - bottomLeftPoint.X;
        int mapHeight = upperRightPoint.Y - bottomLeftPoint.Y;
        RoomType[][] map = CreateBaseMap(mapWidth, mapHeight);
        Point upperRightPointInGridSpace = new Point(mapWidth, mapHeight);

        AddRoomsToMap(ref map, hubRooms);
        AddRoomsToMap(ref map, hallwayRooms);
        AddLinesToMap(ref map, hallwayLines);
        List<MapRoom> fillerRooms = MapRoomTools.CreateRoomsFromFiller(map, mapRoomFactory);
        AddRoomsToMap(ref map, fillerRooms);

        currentState = GenerationState.Finished;

        return new MapData(map, hubRooms, hallwayRooms, fillerRooms, hallwayLines, upperRightPointInGridSpace);
    }

    /// <summary>
    /// Modifies the basic map array with filler ids (1) in line positions.
    /// </summary>
    /// <param name="map">Basic map array</param>
    /// <param name="lines">Lines that will be added to map as filler</param>
    private void AddLinesToMap(ref RoomType[][] map, List<Line> lines)
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
    private void AddRoomsToMap(ref RoomType[][] map, List<MapRoom> rooms)
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
    private RoomType[][] CreateBaseMap(int mapWidth, int mapHeight)
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

    private Point FindUpperRightPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines)
    {
        List<Point> points = new List<Point>()
        {
            MapRoomTools.FindGreatestPointInRooms(hubRooms),
            MapRoomTools.FindGreatestPointInRooms(hallwayRooms),
            MapRoomTools.FindGreatestPointInHallways(hallwayLines),
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

    private Point FindBottomLeftPointInMap(List<MapRoom> hubRooms, List<MapRoom> hallwayRooms, List<Line> hallwayLines)
    {
        List<Point> points = new List<Point>()
        {
            MapRoomTools.FindLowestPointInRooms(hubRooms),
            MapRoomTools.FindLowestPointInRooms(hallwayRooms),
            MapRoomTools.FindLowestPointInHallways(hallwayLines),
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
    /// Creates lines between rooms connected by the delaunay voroni graph line segments.
    /// </summary>
    /// <param name="segments">Graph line segments</param>
    /// <param name="rooms">Rooms with found connections in delaunay voroni graphing step</param>
    /// <returns></returns>
    private List<Line> CreateHallwayLinesFromSegments(List<LineSegment> segments, List<MapRoom> rooms)
    {
        List<Line> hallwayLines = new List<Line>();

        // Buffer size to make hallway lines within room boundries
        int hallwayBuffer = mapSettings.sizeOfHallways / 2;

        foreach (LineSegment segment in segments)
        {
            MapRoom r0;
            MapRoom r1;
            r0 = MapRoomTools.FindRoomContainingPoint(rooms, segment.p0.Value);
            r1 = MapRoomTools.FindRoomContainingPoint(rooms, segment.p1.Value);

            Point midPoint = MapRoomTools.MidPointBetweenMapRooms(r0, r1);

            Vector2 startPoint;
            Vector2 endPoint;

            if (MapRoomTools.IsPointBetweenXBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Stright hallway
            {
                // Create lines from mid point then up and down to rooms.
                startPoint = new Vector2(midPoint.X, r0.centerPoint.y);
                endPoint = new Vector2(midPoint.X, r1.centerPoint.y);

                hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, mapSettings.sizeOfHallways, false));
            }
            else if (MapRoomTools.IsPointBetweenYBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Stright hallway
            {
                // Create lines from mid point then left and right to rooms.
                startPoint = new Vector2(r0.centerPoint.x, midPoint.Y);
                endPoint = new Vector2(r1.centerPoint.x, midPoint.Y);

                hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, mapSettings.sizeOfHallways, true));
            }
            else // Right angle bend in hallway
            {
                // Meeting point between lines
                endPoint = new Vector2(r0.centerPoint.x, r1.centerPoint.y);

                // Is bend in the 'north east' quad?
                bool northEastBend = false;
                if (r0.centerPoint.x > r1.centerPoint.x && r0.centerPoint.y < r1.centerPoint.y)
                    northEastBend = true;

                startPoint = new Vector2(r0.centerPoint.x, r0.centerPoint.y);
                hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, mapSettings.sizeOfHallways, false, northEastBend));

                startPoint = new Vector2(r1.centerPoint.x, r1.centerPoint.y);
                hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, mapSettings.sizeOfHallways, true, northEastBend));
            }
        }

        return hallwayLines;
    }

    /// <summary>
    /// Gets a set number of hallway line segments beteween two rooms with a set width.
    /// </summary>
    /// <param name="startPoint">Point to start the line at</param>
    /// <param name="endPoint">Point to end the line at</param>
    /// <param name="r0">Room 1</param>
    /// <param name="r1">Room 2</param>
    /// <param name="sizeOfHallways">The size of the hallway to make</param>
    /// <param name="isHorizontal">Is the line horizontal or vertical?</param>
    /// <param name="bendInNorthEast">Is the right angle bend 'North East' in relation to the two rooms?</param>
    /// <returns></returns>
    private List<Line> CreateHallwayLinesOfSetWidth(Vector2 startPoint, Vector2 endPoint, int sizeOfHallways, bool isHorizontal, bool bendInNorthEast = false) {
        List<Line> segments = new List<Line>();

        // Add base line
        segments.Add(new Line(startPoint, endPoint));

        // Counter for while loop. Tracks the added width to each line.
        // Starts at 1 since the base line is already in.
        int widthAdded = 1;

        // Distance to spread out the width lines, basically unit in engine.
        int distance = 1;

        // Add lines, then add extra lines to create desired width of hallways.
        while (widthAdded < sizeOfHallways)
        {

            if (isHorizontal) // Line is horizontal
            {
                int bendOffset = widthAdded;
                if (bendInNorthEast)
                {
                    segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                          new Vector2(endPoint.x + bendOffset, endPoint.y + distance)));
                }
                else
                {
                    // Add line for width to positive Vertical side of line.
                    segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                          new Vector2(endPoint.x, endPoint.y + distance)));
                }

                // Step width
                widthAdded++;

                // Add line for width to other side of line
                if (widthAdded < sizeOfHallways) // recheck size
                {
                    if (bendInNorthEast)
                    {
                        segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                              new Vector2(endPoint.x - bendOffset, endPoint.y + distance)));
                    }
                    else
                    {
                        segments.Add(new Line(new Vector2(startPoint.x, startPoint.y - distance),
                                              new Vector2(endPoint.x, endPoint.y - distance)));
                    }
                }
            }
            else // Line is vertical
            {
                int bendOffset = widthAdded;
                if (bendInNorthEast)
                {
                    segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                          new Vector2(endPoint.x + distance, endPoint.y + bendOffset)));
                }
                else
                {
                    // Add line for width to positive Horizontal side of line.
                    segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                          new Vector2(endPoint.x + distance, endPoint.y)));
                }

                // Step width
                widthAdded++;

                // Add line for width to other side of line
                if (widthAdded < sizeOfHallways) // recheck size
                {
                    if (bendInNorthEast)
                    {
                        segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                              new Vector2(endPoint.x + distance, endPoint.y - bendOffset)));
                    }
                    else
                    {
                        segments.Add(new Line(new Vector2(startPoint.x - distance, startPoint.y),
                                              new Vector2(endPoint.x - distance, endPoint.y)));
                    }
                }
            }

            // Step width.
            widthAdded++;
            // Step distance.
            distance++;
        }

        return segments;
    }

    /// <summary>
    /// Updates location of all map rooms by their physical helper objects.
    /// </summary>
    /// <param name="roomHolder">Parent transform that holds physical helper objects</param>
    private void SnapRoomLocationToGrid(Transform roomHolder)
    {
        foreach (Transform child in roomHolder)
        {
            Point location = new Point(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
            child.GetComponent<MapRoomHolder>().mapRoom.gridLocation = location;
        }
    }

    // Removes and resets all created objects to get ready for clean generation
    private void ResetGeneration()
    {
        mapRooms = new List<MapRoom>();

        this.mapRoomFactory = roomFactory;
        this.mapRoomFactory.UpdateSettings(mapSettings);

        RemovePhysicalRoomObjects(this.transform);
    }

    // Resets and regenerates new rooms.
    private void ResetAndRegenerate()
    {
        ResetGeneration();

        Generate();
    }

    // Removes all physical helper room games objects
    private void RemovePhysicalRoomObjects(Transform roomHolder)
    {
        foreach (Transform room in roomHolder)
        {
            Destroy(room.gameObject);
        }
    }


    // Create the physical helper object to utilize the physics engine for room seperation
    private void GeneratePhysicalRooms(List<MapRoom> mapRooms)
    {
        foreach (MapRoom room in mapRooms)
        {
            GameObject physicalRoom = Instantiate(this.physicalRoom);
            physicalRoom.GetComponent<MapRoomHolder>().mapRoom = room;
            physicalRoom.transform.position = new Vector3(room.gridLocation.X, room.gridLocation.Y);
            physicalRoom.transform.localScale = new Vector3(room.width, room.height);
            physicalRoom.transform.SetParent(this.transform, true);
        }
    }

    /// <summary>
    /// Allows physical room objects to seperate the changes state to flag rooms as steady.
    /// </summary>
    /// <param name="roomHolder">Parent object where helper rooms are kept in scene.</param>
    /// <returns></returns>
    private IEnumerator WaitTillRoomsSeperate(Transform roomHolder)
    {
        bool roomsAsleep;

        float savedTimeScale = Time.timeScale;

        Time.timeScale = mapSettings.speedOfPhysicsSeperation;

        // Check all rooms to see if they have settled into place
        do
        { 
            roomsAsleep = true;

            yield return new WaitForSeconds(1f);

            foreach (Transform trans in roomHolder)
            {
                if (trans.GetComponent<Rigidbody2D>().IsAwake())
                {
                    roomsAsleep = false;
                    break;
                }
            }

        } while (!roomsAsleep);

        Time.timeScale = savedTimeScale;

        // Change state to move onto next step.
        currentState = GenerationState.RoomsSeperated;
    }
}
