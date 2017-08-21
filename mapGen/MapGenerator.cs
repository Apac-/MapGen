using Delaunay.Geo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MapGeneration : MonoBehaviour
{
    [SerializeField]
    private MapSettings mapSettings;

    [SerializeField]
    private GameObject physicalRoom;

    private enum GenerationState { Waiting, RoomsSeparated, Reset, Finished }
    private GenerationState currentState;

    private IMapRoomFactory mapRoomFactory;
    private IMapDataFactory mapDataFactory;
    private IHallwayFactory hallwayFactory;
    private IPhysicalMapRoomFactory physMapRoomFactory;

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
                          IPhysicalMapRoomFactory physMapRoomFactory,
                          IMapRoomTools mapRoomTools,
                          IPointTriangulation pointTriangulation)
    {
        this.mapRoomFactory = mapRoomFactory;
        this.mapDataFactory = mapDataFactory;
        this.hallwayFactory = hallwayFactory;
        this.physMapRoomFactory = physMapRoomFactory;
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
            case GenerationState.Reset:
                Generate();
                break;
            case GenerationState.RoomsSeparated:
                WorkWithSeparatedRooms();
                break;
            case GenerationState.Finished:
                CleanUp();
                currentState = GenerationState.Waiting;
                break;
        }
    }

    /// <summary>
    /// Triggers the next step in generation after physical room objects have fully separated.
    /// </summary>
    private void WorkWithSeparatedRooms()
    {
        MapData mapData = GenerateMap();

        if (mapData != null)
        {
            visualDebugger.SetMapData(mapData);
            currentState = GenerationState.Finished;
        }
        else
            currentState = GenerationState.Reset;
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

        List<MapRoom> mapRooms = mapRoomFactory.CreateRooms();

        physMapRoomFactory.GeneratePhysicalRooms(this.transform, physicalRoom, mapRooms);

        StartCoroutine(WaitTillRoomsSeperate(physMapRoomFactory));
    }

    /// <summary>
    /// Used after physical rooms have seperated (touching but not overlapping) to create map data from their locations.
    /// </summary>
    /// <param name="rooms">Rooms that have been seperated (not overlapping)</param>
    /// <returns></returns>
    private MapData GenerateMap()
    {
        // Snap rooms to grid space from world space of physical helper rooms
        List<MapRoom> rooms = physMapRoomFactory.SnapMapRoomLocationToPhysicalRoomLocation();

        List<MapRoom> hubRooms = mapRoomTools.FindHubRooms(rooms, mapSettings.hubRoomCutoff);

        // If not enough hub rooms are found, return null as this pass has failed
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

    private void CleanUp()
    {
        physMapRoomFactory.RemovePhysicalRooms();
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
    private IEnumerator WaitTillRoomsSeperate(IPhysicalMapRoomFactory physMapRoomTools)
    {
        float savedTimeScale = Time.timeScale;

        Time.timeScale = mapSettings.speedOfPhysicsSeperation;

        while(physMapRoomTools.RoomsHaveSeparated() == false)
        {
            yield return new WaitForSeconds(1f);
        }

        Time.timeScale = savedTimeScale;

        // Change state to move onto next step.
        currentState = GenerationState.RoomsSeparated;
    }
}