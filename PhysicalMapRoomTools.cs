using System.Collections.Generic;
using UnityEngine;

public class PhysicalMapRoomTools : IPhysicalMapRoomTools
{
    /// <summary>
    /// Create the physical helper object to utilize the physics engine for room seperation
    /// </summary>
    /// <param name="parent">Parent transform that will hold the newly created objects</param>
    /// <param name="roomPrefab">Prefab of helper room object</param>
    /// <param name="mapRooms">MapRoom holds the data that will be used later after, based on attributes of the created physical helper objects</param>
    public void GeneratePhysicalRooms(Transform parent, GameObject roomPrefab, List<MapRoom> mapRooms)
    {
        foreach (MapRoom room in mapRooms)
        {
            GameObject physicalRoom = GameObject.Instantiate(roomPrefab);
            physicalRoom.GetComponent<MapRoomHolder>().mapRoom = room;
            physicalRoom.transform.position = new Vector3(room.gridLocation.X, room.gridLocation.Y);
            physicalRoom.transform.localScale = new Vector3(room.width, room.height);
            physicalRoom.transform.SetParent(parent, true);
        }
    }

    /// <summary>
    /// Remove all healer room objects from parent.
    /// </summary>
    /// <param name="parent">Physcial room helper object parent</param>
    public void RemovePhysicalRooms(Transform parent)
    {
        foreach (Transform room in parent)
        {
            GameObject.Destroy(room.gameObject);
        }
    }

    /// <summary>
    /// Updates location of all map rooms by their physical helper objects.
    /// </summary>
    /// <param name="roomHolder">Parent transform that holds physical helper objects</param>
    public void SnapMapRoomLocationToPhysicalRoomLocation(Transform roomHolder)
    {
        foreach (Transform child in roomHolder)
        {
            Point location = new Point(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
            child.GetComponent<MapRoomHolder>().mapRoom.gridLocation = location;
        }
    }
}
