using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MapGen : MonoBehaviour {
    [SerializeField]
    private MapSettings mapSettings;

    [SerializeField]
    private GameObject physicalRoom;

    private enum GenerationState { Waiting, RoomsSeperated, Reset, Finished }
    private GenerationState currentState;

    private List<MapRoom> mapRooms;

    private IMapRoomFactory mapRoomFactory;
    private IPhysicalMapRoomTools physMapRoomTools;
    private IMapRoomTools mapRoomTools;
    private IPointTriangulation pointTriangulation;

    // NOTE: This I wouldn't hold in a real project, instead it would subscribe to an event thrown from this object.
    private MapGenVisualDebugger visualDebugger;

    // Use this for initialization
    void Start () {
        currentState = GenerationState.Waiting;
        visualDebugger = gameObject.GetComponent<MapGenVisualDebugger>();
	}


    // Constructor type method to inject into via zenject installer.
    [Inject]
    public void Construct(IMapRoomFactory mapRoomFactory,
                          IPhysicalMapRoomTools physMapRoomTools,
                          IMapRoomTools mapRoomTools,
                          IPointTriangulation pointTriangulation)
    {
        this.mapRoomFactory = mapRoomFactory;
        this.physMapRoomTools = physMapRoomTools;
        this.mapRoomTools = mapRoomTools;
        this.pointTriangulation = pointTriangulation;
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
                Generate();
                break;
            case GenerationState.Finished:
                CleanUp();
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

        physMapRoomTools.GeneratePhysicalRooms(this.transform, physicalRoom, mapRooms);

        StartCoroutine(WaitTillRoomsSeperate(this.transform));
    }

    /// <summary>
    /// Used after physical rooms have seperated (touching but not overlapping) to create map data from their locations.
    /// </summary>
    /// <param name="rooms">Rooms that have been seperated (not overlapping)</param>
    /// <returns></returns>
    private MapData GenerateMap(List<MapRoom> rooms)
    {
        physMapRoomTools.SnapMapRoomLocationToPhysicalRoomLocation(this.transform);

        List<MapRoom> hubRooms = mapRoomTools.FindHubRooms(rooms, mapSettings.hubRoomCutoff);

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

        List<Line> connectingLineSegments = pointTriangulation.FindConnectingLineSegments(hubRoomCenterPoints, 
                                                                                          mapSettings.percentOfRoomConnectionAboveMinPath);

        List<Line> hallwayLines = CreateHallwayLinesFromSegments(connectingLineSegments, hubRooms);

        List<MapRoom> hallwayRooms = mapRoomTools.FindHallwayRooms(hallwayLines, hubRooms);


        return new MapData(hubRooms, hallwayRooms, hallwayLines, mapRoomFactory);


        currentState = GenerationState.Finished;

        //return new MapData(map, hubRooms, hallwayRooms, fillerRooms, hallwayLines, upperRightPointInGridSpace);
    }


    /// <summary>
    /// Creates lines between rooms connected by the delaunay voroni graph line segments.
    /// </summary>
    /// <param name="segments">Graph line segments</param>
    /// <param name="rooms">Rooms with found connections in delaunay voroni graphing step</param>
    /// <returns></returns>
    private List<Line> CreateHallwayLinesFromSegments(List<Line> segments, List<MapRoom> rooms)
    {
        List<Line> hallwayLines = new List<Line>();

        // Buffer size to make hallway lines within room boundries
        int hallwayBuffer = mapSettings.sizeOfHallways / 2;

        foreach (Line segment in segments)
        {
            MapRoom r0;
            MapRoom r1;
            r0 = mapRoomTools.FindRoomContainingPoint(rooms, segment.p0);
            r1 = mapRoomTools.FindRoomContainingPoint(rooms, segment.p1);

            Point midPoint = mapRoomTools.MidPointBetweenMapRooms(r0, r1);

            Vector2 startPoint;
            Vector2 endPoint;

            if (mapRoomTools.IsPointBetweenXBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Stright hallway
            {
                // Create lines from mid point then up and down to rooms.
                startPoint = new Vector2(midPoint.X, r0.centerPoint.y);
                endPoint = new Vector2(midPoint.X, r1.centerPoint.y);

                hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, mapSettings.sizeOfHallways, false));
            }
            else if (mapRoomTools.IsPointBetweenYBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Stright hallway
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

    // Removes the room helper objects from scene and resets mapRooms list.
    private void CleanUp()
    {
        mapRooms = new List<MapRoom>();

        physMapRoomTools.RemovePhysicalRooms(this.transform);
    }

    // Removes and resets all created objects to get ready for clean generation
    private void ResetGeneration()
    {
        CleanUp();

        mapRoomFactory.UpdateSettings(mapSettings);
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
