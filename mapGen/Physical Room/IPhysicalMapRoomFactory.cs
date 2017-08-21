using System.Collections.Generic;
using UnityEngine;

namespace MapGen
{
    public interface IPhysicalMapRoomFactory
    {
        void GeneratePhysicalRooms(Transform parent, GameObject roomPrefab, List<MapRoom> mapRooms);
        void RemovePhysicalRooms();
        List<MapRoom> SnapMapRoomLocationToPhysicalRoomLocation();
        bool RoomsHaveSeparated();
    }
}