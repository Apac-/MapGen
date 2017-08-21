using System.Collections.Generic;
using UnityEngine;

public interface IPhysicalMapRoomFactory
{
    void GeneratePhysicalRooms(Transform parent, GameObject roomPrefab, List<MapRoom> mapRooms);
    void RemovePhysicalRooms();
    void SnapMapRoomLocationToPhysicalRoomLocation();
    bool RoomsHaveSeparated();
}