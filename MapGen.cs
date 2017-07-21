using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour {

    // Used in adding more connecting paths after the min amount is found to connect all rooms.
    public float percentOfRoomConnectionAboveMinPath;

    // Speeds up the seperation of phys engine to make generation quicker
    public float speedOfPhysicsSeperation;

    // We want at least this many hub rooms created
    public int minAmountOfHubRooms;

    // Used to find the larger rooms to be used as hub rooms
    public float hubRoomCutoff = 1.25f;

    // The size, in tiles, of how wide a hallway should be.
    public int sizeOfHallways;

    // Room variables
    public int numberOfRoomsToCreate;
    public int roomSpawnEllipsisAreaWidth;
    public int roomSpawnEllipsisAreaHeight;
    public int roomMeanWidth;
    public int roomMeanHeight;
    public int roomStandardDeviation;
    public int roomMaxWidth;
    public int roomMinWidth;
    public int roomMaxHeight;
    public int roomMinHeight;

    private enum GenerationState { Waiting, RoomsSeperated,}
    private GenerationState currentState;

    private List<MapRoom> mapRooms;

    // Use this for initialization
    void Start () {
        currentState = GenerationState.Waiting;
	}
	
	// Update is called once per frame
	void Update () {
    }

    private void GenerateMap(List<MapRoom> rooms)
    {
        SnapRoomLocationToGrid(this.transform);

        List<MapRoom> hubRooms = GetHubRooms(rooms, hubRoomCutoff);

        // Re-generate if not enough hub rooms are found
        if (hubRooms.Count <= minAmountOfHubRooms)
        {
            throw new NotImplementedException();
            currentState = GenerationState.Reset;
            return;
        }

        List<Vector2> hubRoomCenterPoints = new List<Vector2>();
        foreach (MapRoom room in hubRooms)
        {
            hubRoomCenterPoints.Add(room.centerPoint);
        }

        List<LineSegment> connectingLineSegments = DelaunayGrapher.FindConnectingLineSegments(hubRoomCenterPoints, percentOfRoomConnectionAboveMinPath);

        List<Line> hallwayLines = CreateHallwayLinesFromSegments(connectingLineSegments, hubRooms);
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

        int hallwayBuffer = sizeOfHallways / 2;

        // Switch that changes up the direction of hall way drawing.
        // Makes for a better visual on end product.
        bool switchDirectionOfLines = false; 

        foreach (LineSegment segment in segments)
        {
            MapRoom r0;
            MapRoom r1;
            r0 = FindRoomContainingPoint(rooms, segment.p0.Value);
            r1 = FindRoomContainingPoint(rooms, segment.p1.Value);

            Point midPoint = MapRoom.MidPointBetweenMapRooms(r0, r1);

            Vector2 startPoint;
            Vector2 endPoint;

            if (MapRoom.IsPointBetweenXBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer))
            {
                // Create lines from mid point then up and down to rooms.
                startPoint = new Vector2(midPoint.X, r0.centerPoint.y);
                endPoint = new Vector2(midPoint.X, r1.centerPoint.y);

                hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, false));
            }
            else if (MapRoom.IsPointBetweenYBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer))
            {
                // Create lines from mid point then left and right to rooms.
                startPoint = new Vector2(r0.centerPoint.x, midPoint.Y);
                endPoint = new Vector2(r1.centerPoint.x, midPoint.Y);

                hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, true));
            }
            else
            {
                int lineAdjustmentY = r0.centerPoint.y > r1.centerPoint.y ? 1 : -1;
                int lineAdjustmentX = r0.centerPoint.x > r1.centerPoint.x ? 1 : -1;

                if (switchDirectionOfLines)
                {
                    startPoint = new Vector2(r0.centerPoint.x + lineAdjustmentX, r0.centerPoint.y);
                    endPoint = new Vector2(r1.centerPoint.x - lineAdjustmentX, r0.centerPoint.y);
                    hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, true));

                    startPoint = new Vector2(r1.centerPoint.x, r1.centerPoint.y - lineAdjustmentY);
                    endPoint = new Vector2(r1.centerPoint.x, r0.centerPoint.y + lineAdjustmentY);
                    hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, false));
                }
                else
                {
                    startPoint = new Vector2(r0.centerPoint.x, r0.centerPoint.y + lineAdjustmentY);
                    endPoint = new Vector2(r0.centerPoint.x, r1.centerPoint.y - lineAdjustmentY);
                    hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, false));

                    startPoint = new Vector2(r1.centerPoint.x - lineAdjustmentX, r1.centerPoint.y);
                    endPoint = new Vector2(r0.centerPoint.x + lineAdjustmentX, r1.centerPoint.y);
                    hallwayLines.AddRange(GetHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, true));
                }

                // Flip the line direciton.
                switchDirectionOfLines = !switchDirectionOfLines;
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
    /// <returns></returns>
    private List<Line> GetHallwayLinesOfSetWidth(Vector2 startPoint, Vector2 endPoint, int sizeOfHallways, bool isHorizontal) {
        List<Line> segments = new List<Line>();

        // Add base line
        segments.Add(new Line(startPoint, endPoint));

        // Counter for while loop. Tracks the added width to each line.
        // Starts at 1 since the base line is already in.
        int widthAdded = 1;

        // Distance to spread out the width lines, basically unit in engine.
        int distance = 1;

        // Add lines, then add extra lines to create desired width of hallways.
        while (widthAdded < sizeOfHallways) {

            if (isHorizontal) { // Line is horizontal
                // Add line for width to positive Vertical side of line.
                segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                      new Vector2(endPoint.x, endPoint.y + distance)));

                // Step width
                widthAdded++;

                // Add line for width to other side of line
                if (widthAdded < sizeOfHallways) { // recheck size
                    segments.Add(new Line(new Vector2(startPoint.x, startPoint.y - distance),
                                          new Vector2(endPoint.x, endPoint.y - distance)));
                }
            }
            else { // Line is vertical
                // Add line for width to positive Horizontal side of line.
                segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                      new Vector2(endPoint.x + distance, endPoint.y)));

                // Step width
                widthAdded++;

                // Add line for width to other side of line
                if (widthAdded < sizeOfHallways) { // recheck size
                    segments.Add(new Line(new Vector2(startPoint.x - distance, startPoint.y),
                                          new Vector2(endPoint.x - distance, endPoint.y)));
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
    /// Searches given rooms to find one that contains the given point.
    /// Its assumed that rooms do not overlap thus each room contains unique points.
    /// </summary>
    /// <param name="rooms">Map rooms to search through.</param>
    /// <param name="point">Point that should be contained in one of the given rooms.</param>
    /// <returns></returns>
    private MapRoom FindRoomContainingPoint(List<MapRoom> rooms, Vector2 point)
    {
        foreach (MapRoom room in rooms)
        {
            if (room.ContainsPoint(point))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds and returns all rooms that are large enough to be main or 'hub' rooms.
    /// </summary>
    /// <param name="rooms">Rooms to check for hubs</param>
    /// <param name="cutoff">Adjustment to further limit allowed lower size of hub rooms</param>
    /// <returns></returns>
    private List<MapRoom> GetHubRooms(List<MapRoom> rooms, float cutoff)
    {
        int height_mean = 0;
        int width_mean = 0;

        foreach (MapRoom room in rooms) {
            height_mean += room.height;
            width_mean += room.width;
        }

        height_mean = Mathf.RoundToInt((float)(height_mean / rooms.Count) * cutoff);
        width_mean = Mathf.RoundToInt((float)(width_mean / rooms.Count) * cutoff);

        List<MapRoom> hubRooms = new List<MapRoom>();
        foreach (MapRoom room in rooms) {
            if (room.width > width_mean && room.height > height_mean)
                hubRooms.Add(room);
        }

        return hubRooms;
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

        RemovePhysicalRoomObjects(this.transform);
    }

    // Removes all physical helper room games objects
    private void RemovePhysicalRoomObjects(Transform roomHolder)
    {
        foreach (Transform room in roomHolder)
        {
            Destroy(room.gameObject);
        }
    }

    // Create the foundation map rooms and physical helper rooms then wait till the phys engine seperates helpers
    public void GenerateRooms()
    {
        mapRooms = GenerateMapRooms();

        GeneratePhysicalRooms(mapRooms);

        StartCoroutine(WaitTillRoomsSeperate(this.transform.parent));
    }

    // Create the physical helper object to utilize the physics engine for room seperation
    private void GeneratePhysicalRooms(List<MapRoom> mapRooms)
    {
        foreach (MapRoom room in mapRooms)
        {
            GameObject physicalRoom = new GameObject("PhysicalRoom", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(MapRoomHolder));
            physicalRoom.GetComponent<MapRoomHolder>().mapRoom = room;
            physicalRoom.transform.position = new Vector3(room.gridLocation.X, room.gridLocation.Y);
            physicalRoom.transform.localScale = new Vector3(room.width, room.height);
            physicalRoom.transform.SetParent(this.transform.parent, true);
        }
    }

    private List<MapRoom> GenerateMapRooms()
    {
        MapRoomCreator roomCreator = new MapRoomCreator(numberOfRoomsToCreate, 
            roomSpawnEllipsisAreaWidth, roomSpawnEllipsisAreaHeight,
            roomMeanHeight, roomMeanWidth, roomStandardDeviation, 
            roomMaxWidth, roomMinWidth, roomMaxHeight, roomMinHeight);

        List<MapRoom> rooms = roomCreator.CreateRooms();

        return rooms;
    }

    private IEnumerator WaitTillRoomsSeperate(Transform roomHolder)
    {
        bool roomsAsleep;

        float savedTimeScale = Time.timeScale;

        Time.timeScale = speedOfPhysicsSeperation;

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
    }
}
