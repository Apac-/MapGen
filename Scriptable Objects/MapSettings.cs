using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Helpers/MapSettings", order = 1)]
public class MapSettings : ScriptableObject {
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
}
