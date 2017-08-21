using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGen
{
    public class MapRoom
    {
        public RoomType roomType;

        public int Id { get; private set; }

        // Location is bottom left of the room
        public Point gridLocation { get; set; }

        public Vector2 centerPoint { get { return new Vector2(gridLocation.X + (width / 2), gridLocation.Y + (height / 2)); } }

        public Point endPoint { get { return new Point(gridLocation.X + width, gridLocation.Y + height); } }

        public int width { get; private set; }
        public int height { get; private set; }

        public MapRoom(Point location, int width, int height, int id)
        {
            Id = id;
            this.width = width;
            this.height = height;
            gridLocation = location;

            roomType = RoomType.Empty;
        }

        /// <summary>
        /// Gets all points in grid space that this room contains.
        /// </summary>
        /// <returns></returns>
        public List<Point> ContainedPoints()
        {
            List<Point> points = new List<Point>();
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    points.Add(new Point(gridLocation.X + w, gridLocation.Y + h));
                }
            }

            return points;
        }

        /// <summary>
        /// Returns true if given point is within room.
        /// </summary>
        /// <param name="p">The given point to find</param>
        /// <returns></returns>
        public bool ContainsPoint(Point p)
        {

            if (p.X >= gridLocation.X &&
                p.X <= endPoint.X &&
                p.Y >= gridLocation.Y &&
                p.Y <= endPoint.Y)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if given point is within room.
        /// </summary>
        /// <param name="p">Vector2 point to find</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 p)
        {
            return ContainsPoint(new Point((int)p.x, (int)p.y));
        }
    }
}