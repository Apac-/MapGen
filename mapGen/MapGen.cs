using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MapGen : MonoBehaviour
{
    [SerializeField]
    private MapSettings mapSettings;

    [SerializeField]
    private GameObject physicalRoom;

    private enum GenerationState { Waiting, RoomsSeparated, Reset, Finished }
    private GenerationState currentState;

    private List<MapRoom> mapRooms;

    private IMapRoomFactory mapRoomFactory;
    private IMapDataFactory mapDataFactory;
    private IHallwayFactory hallwayFactory;

    private IPhysicalMapRoomTools physMapRoomTools;
    private IMapRoomTools mapRoomTools;

    private IPointTriangulation pointTriangulation;

    // NOTE: This I wouldn't hold in a real project, instead it would subscribe to an event thrown from this object.
    private MapGenVisualDebugger visualDebugger;

    // Use this for initialization
    void Start()
    {
        currentState = GenerationState.Waiting;
        visualDebugger = gameObject.GetComponent<MapGenVisualDebugger>();
    }


    // Constructor type method to inject into via zenject installer.
    [Inject]
    public void Construct(IMapRoomFactory mapRoomFactory,
                          IMapDataFactory mapDataFactory,
                          IHallwayFactory hallwayFactory,
                          IPhysicalMapRoomTools physMapRoomTools,
                          IMapRoomTools mapRoomTools,
                          IPointTriangulation pointTriangulation)
    {
        this.mapRoomFactory = mapRoomFactory;
        this.mapDataFactory = mapDataFactory;
        this.hallwayFactory = hallwayFactory;
        this.physMapRoomTools = physMapRoomTools;
        this.mapRoomTools = mapRoomTools;
        this.pointTriangulation = pointTriangulation;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GenerationState.Waiting:
                break;
            case GenerationState.RoomsSeparated:
                // TODO: This doesn't seem right.
                MapData mapData = GenerateMap(mapRooms);
                if (mapData != null)
                {
                    visualDebugger.SetMapData(mapData);
                    currentState = GenerationState.Finished;
                }
                else
                    currentState = GenerationState.Reset;
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
            return null;
        }

        List<Vector2> hubRoomCenterPoints = new List<Vector2>();
        foreach (MapRoom room in hubRooms)
        {
            hubRoomCenterPoints.Add(room.centerPoint);
        }

        List<Line> connectingLineSegments = pointTriangulation.FindConnectingLineSegments(hubRoomCenterPoints,
                                                                                          mapSettings.percentOfRoomConnectionAboveMinPath);

        List<Line> hallwayLines = hallwayFactory.CreateHallwayLinesFromSegments(connectingLineSegments, hubRooms, mapSettings.sizeOfHallways, mapRoomTools);

        List<MapRoom> hallwayRooms = mapRoomTools.FindHallwayRooms(hallwayLines, hubRooms);

        return mapDataFactory.CreateNewMapData(hubRooms, hallwayRooms, hallwayLines, mapRoomFactory);
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
        currentState = GenerationState.RoomsSeparated;
    }
}