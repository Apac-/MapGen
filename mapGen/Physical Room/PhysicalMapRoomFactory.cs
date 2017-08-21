using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalMapRoomFactory : IPhysicalMapRoomFactory
{
    private List<GameObject> physicalRooms;

    /// <summary>
    /// Create the physical helper object to utilize the physics engine for room seperation
    /// </summary>
    /// <param name="parent">Parent transform that will hold the newly created objects</param>
    /// <param name="roomPrefab">Prefab of helper room object</param>
    /// <param name="mapRooms">MapRoom holds the data that will be used later after, based on attributes of the created physical helper objects</param>
    public void GeneratePhysicalRooms(Transform parent, GameObject roomPrefab, List<MapRoom> mapRooms)
    {
        if (physicalRooms != null)
            RemovePhysicalRooms();

        physicalRooms = new List<GameObject>();

        foreach (MapRoom room in mapRooms)
        {
            GameObject physicalRoom = GameObject.Instantiate(roomPrefab);

            if (physicalRoom.GetComponent<Rigidbody2D>() == null)
                throw new ArgumentNullException("GetComponent Rigidbody2D", "Prefab must contain a Rigidbody2D component.");
            if (physicalRoom.GetComponent<MapRoomHolder>() == null)
                throw new ArgumentNullException("GetComponent MapRoomHolder", "Prefab must contain a MapRoomHolder component.");

            physicalRoom.GetComponent<MapRoomHolder>().mapRoom = room;
            physicalRoom.transform.position = new Vector3(room.gridLocation.X, room.gridLocation.Y);
            physicalRoom.transform.localScale = new Vector3(room.width, room.height);
            physicalRoom.transform.SetParent(parent, true);

            physicalRooms.Add(physicalRoom);
        }
    }

    /// <summary>
    /// Destroy all physical rooms.
    /// </summary>
    public void RemovePhysicalRooms()
    {
        if (physicalRooms == null)
            return;

        foreach (GameObject room in physicalRooms)
        {
            GameObject.Destroy(room);
        }

        physicalRooms = null;
    }

    /// <summary>
    /// Checks if the rooms have fully separated and settled.
    /// </summary>
    /// <returns></returns>
    public bool RoomsHaveSeparated()
    {
        if (physicalRooms == null)
            throw new ArgumentNullException("Physical Rooms", "Physical Rooms have not been created yet. Use GeneratePhysicalRooms first");

        foreach (GameObject room in physicalRooms)
        {
            if (room.GetComponent<Rigidbody2D>().IsAwake())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Updates location of all map rooms by their physical helper objects.
    /// </summary>
    public void SnapMapRoomLocationToPhysicalRoomLocation()
    {
        if (physicalRooms == null)
            throw new ArgumentNullException("Physical Rooms", "Physical Rooms have not been created yet. Use GeneratePhysicalRooms first");

        foreach (GameObject room in physicalRooms)
        {
            Point location = new Point(Mathf.RoundToInt(room.transform.position.x), Mathf.RoundToInt(room.transform.position.y));
            room.GetComponent<MapRoomHolder>().mapRoom.gridLocation = location;
        }
    }
}