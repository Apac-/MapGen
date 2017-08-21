using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGen
{
    public class MapGenVisualDebugger : MonoBehaviour
    {

        #region Change these in editor
        public bool debugMap;

        public bool drawHallWayLines;
        public Color hallwayLineColor;

        public bool drawHallwayFillerTiles;
        public Color hallwayFillerTilesColor;

        public bool drawHubRooms;
        public Color hubRoomColor;

        public bool drawHallwayRooms;
        public Color hallwayRoomColor;

        public bool drawFillerRooms;
        public Color fillerRoomColor;

        public bool drawMapBounds;
        public Color mapBoundsColor;
        #endregion

        Vector2[] mapBounds = new Vector2[2];

        private List<Vector2> hallwayFillerTiles;

        private MapData mapData;

        // Update is called once per frame
        void Update()
        {
            if (debugMap && mapData != null)
                DrawDebugLines();
        }

        public void SetMapData(MapData mapData)
        {
            this.mapData = mapData;
            SetUpLinesToDraw(this.mapData);
        }

        private void SetUpLinesToDraw(MapData mapData)
        {
            mapBounds = SetBoundries(mapData.greatestPoint);
            hallwayFillerTiles = FindHallwayFillerTiles(mapData.map);
        }

        private void DrawDebugLines()
        {
            if (drawHallWayLines)
                DrawLines(mapData.hallwayLines, hallwayLineColor);
            if (drawHallwayFillerTiles)
                DrawIndividualTiles(hallwayFillerTiles, hallwayFillerTilesColor);
            if (drawMapBounds)
                DrawMapBounds(mapBounds, mapBoundsColor);
            if (drawFillerRooms)
                DrawRooms(mapData.fillerRooms, fillerRoomColor);
            if (drawHallwayRooms)
                DrawRooms(mapData.hallwayRooms, hallwayRoomColor);
            if (drawHubRooms)
                DrawRooms(mapData.hubRooms, hubRoomColor);
        }

        private void DrawIndividualTiles(List<Vector2> tiles, Color color)
        {
            foreach (Vector2 tile in tiles)
            {
                Vector3 bottomLeft = new Vector3(tile.x, tile.y);
                Vector3 bottomRight = new Vector3(tile.x + 1, tile.y);
                Vector3 topRight = new Vector3(tile.x + 1, tile.y + 1);
                Vector3 topLeft = new Vector3(tile.x, tile.y + 1);

                Debug.DrawLine(bottomLeft, bottomRight, color);
                Debug.DrawLine(bottomLeft, topLeft, color);
                Debug.DrawLine(topRight, bottomRight, color);
                Debug.DrawLine(topRight, topLeft, color);
            }
        }

        private void DrawLines(List<Line> lines, Color color)
        {
            foreach (Line line in lines)
            {
                Debug.DrawLine(line.p0, line.p1, color);
            }
        }

        private void DrawRooms(List<MapRoom> rooms, Color color)
        {
            foreach (MapRoom room in rooms)
            {
                Vector3 bottomLeft = new Vector3(room.gridLocation.X, room.gridLocation.Y);
                Vector3 bottomRight = new Vector3(room.gridLocation.X + room.width, bottomLeft.y);
                Vector3 topRight = new Vector3(room.endPoint.X, room.endPoint.Y);
                Vector3 topLeft = new Vector3(room.gridLocation.X, room.gridLocation.Y + room.height);
                Debug.DrawLine(bottomLeft, bottomRight, color);
                Debug.DrawLine(bottomLeft, topLeft, color);
                Debug.DrawLine(bottomRight, topRight, color);
                Debug.DrawLine(topLeft, topRight, color);
            }
        }

        private List<Vector2> FindHallwayFillerTiles(RoomType[][] map)
        {
            List<Vector2> fillerTiles = new List<Vector2>();

            for (int width = 0; width < map.Length; width++)
            {
                for (int height = 0; height < map[width].Length; height++)
                {
                    if (map[width][height] == RoomType.Filler)
                        fillerTiles.Add(new Vector2(width, height));
                }
            }

            return fillerTiles;
        }

        private List<Vector2[]> FindHallWayLines(List<Line> lines)
        {
            List<Vector2[]> ret = new List<Vector2[]>();
            foreach (Line line in lines)
            {
                ret.Add(new Vector2[] { line.p0, line.p1 });
            }

            return ret;
        }

        private Vector2[] SetBoundries(Point greatestPoint)
        {
            Vector2[] bounds = new Vector2[2];

            bounds[0] = new Vector2(0, 0);
            bounds[1] = new Vector2(greatestPoint.X, greatestPoint.Y);

            return bounds;
        }


        private void DrawMapBounds(Vector2[] bounds, Color color)
        {
            Vector3 bottomLeft = new Vector3(bounds[0].x, bounds[0].y);
            Vector3 bottomRight = new Vector3(bounds[1].x, bounds[0].y);
            Vector3 topRight = new Vector3(bounds[1].x, bounds[1].y);
            Vector3 topLeft = new Vector3(bounds[0].x, bounds[1].y);
            Debug.DrawLine(bottomLeft, bottomRight, color);
            Debug.DrawLine(bottomLeft, topLeft, color);
            Debug.DrawLine(topRight, bottomRight, color);
            Debug.DrawLine(topRight, topLeft, color);
        }
    }
}