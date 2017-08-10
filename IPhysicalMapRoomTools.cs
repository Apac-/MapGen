using System.Collections.Generic;
using UnityEngine;

public interface IPhysicalMapRoomTools
{
    void GeneratePhysicalRooms(Transform parent, GameObject roomPrefab, List<MapRoom> mapRooms);

    void RemovePhysicalRooms(Transform parent);

    void SnapMapRoomLocationToPhysicalRoomLocation(Transform roomHolder);
}