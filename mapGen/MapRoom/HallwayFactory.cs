using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapGen
{
    public class HallwayFactory : IHallwayFactory
    {
        /// <summary>
        /// Creates lines between rooms connected by line segments.
        /// </summary>
        /// <param name="connectingLineSegments">Graph line segments</param>
        /// <param name="rooms">Rooms with found connections in point triangulation connected tree</param>
        /// <param name="sizeOfHallways">Girth of the hallways</param>
        /// <param name="mapRoomTools">Room tools</param>
        /// <returns></returns>
        public List<Line> CreateHallwayLinesFromSegments(List<Line> connectingLineSegments, List<MapRoom> rooms, int sizeOfHallways, IMapRoomTools mapRoomTools)
        {
            List<Line> hallwayLines = new List<Line>();

            // Buffer size to make hallway lines within room boundries
            int hallwayBuffer = sizeOfHallways / 2;

            foreach (Line segment in connectingLineSegments)
            {
                MapRoom r0;
                MapRoom r1;
                r0 = mapRoomTools.FindRoomContainingPoint(rooms, segment.p0);
                r1 = mapRoomTools.FindRoomContainingPoint(rooms, segment.p1);

                Point midPoint = mapRoomTools.MidPointBetweenMapRooms(r0, r1);

                Vector2 startPoint;
                Vector2 endPoint;

                if (mapRoomTools.IsPointBetweenXBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Straight hallway
                {
                    // Create lines from mid point then up and down to rooms.
                    startPoint = new Vector2(midPoint.X, r0.centerPoint.y);
                    endPoint = new Vector2(midPoint.X, r1.centerPoint.y);

                    hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, false));
                }
                else if (mapRoomTools.IsPointBetweenYBoundariesOfGivenRooms(midPoint, r0, r1, hallwayBuffer)) // Straight hallway
                {
                    // Create lines from mid point then left and right to rooms.
                    startPoint = new Vector2(r0.centerPoint.x, midPoint.Y);
                    endPoint = new Vector2(r1.centerPoint.x, midPoint.Y);

                    hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, true));
                }
                else // Right angle bend in hallway
                {
                    // Meeting point between lines
                    endPoint = new Vector2(r0.centerPoint.x, r1.centerPoint.y);

                    // Is bend in the 'north east' quad?
                    bool northEastBend = false;
                    if (r0.centerPoint.x > r1.centerPoint.x && r0.centerPoint.y < r1.centerPoint.y)
                        northEastBend = true;

                    startPoint = new Vector2(r0.centerPoint.x, r0.centerPoint.y);
                    hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, false, northEastBend));

                    startPoint = new Vector2(r1.centerPoint.x, r1.centerPoint.y);
                    hallwayLines.AddRange(CreateHallwayLinesOfSetWidth(startPoint, endPoint, sizeOfHallways, true, northEastBend));
                }
            }

            return hallwayLines;
        }

        /// <summary>
        /// Gets a set number of hallway line segments beteween two rooms with a set width.
        /// </summary>
        /// <param name="startPoint">Point to start the line at</param>
        /// <param name="endPoint">Point to end the line at</param>
        /// <param name="r0">Room 1</param>
        /// <param name="r1">Room 2</param>
        /// <param name="sizeOfHallways">The size of the hallway to make</param>
        /// <param name="isHorizontal">Is the line horizontal or vertical?</param>
        /// <param name="bendInNorthEast">Is the right angle bend 'North East' in relation to the two rooms?</param>
        /// <returns></returns>
        private List<Line> CreateHallwayLinesOfSetWidth(Vector2 startPoint, Vector2 endPoint, int sizeOfHallways, bool isHorizontal, bool bendInNorthEast = false)
        {
            List<Line> segments = new List<Line>();

            // Add base line
            segments.Add(new Line(startPoint, endPoint));

            // Counter for while loop. Tracks the added width to each line.
            // Starts at 1 since the base line is already in.
            int widthAdded = 1;

            // Distance to spread out the width lines, basically unit in engine.
            int distance = 1;

            // Add lines, then add extra lines to create desired width of hallways.
            while (widthAdded < sizeOfHallways)
            {

                if (isHorizontal) // Line is horizontal
                {
                    int bendOffset = widthAdded;
                    if (bendInNorthEast)
                    {
                        segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                              new Vector2(endPoint.x + bendOffset, endPoint.y + distance)));
                    }
                    else
                    {
                        // Add line for width to positive Vertical side of line.
                        segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                              new Vector2(endPoint.x, endPoint.y + distance)));
                    }

                    // Step width
                    widthAdded++;

                    // Add line for width to other side of line
                    if (widthAdded < sizeOfHallways) // recheck size
                    {
                        if (bendInNorthEast)
                        {
                            segments.Add(new Line(new Vector2(startPoint.x, startPoint.y + distance),
                                                  new Vector2(endPoint.x - bendOffset, endPoint.y + distance)));
                        }
                        else
                        {
                            segments.Add(new Line(new Vector2(startPoint.x, startPoint.y - distance),
                                                  new Vector2(endPoint.x, endPoint.y - distance)));
                        }
                    }
                }
                else // Line is vertical
                {
                    int bendOffset = widthAdded;
                    if (bendInNorthEast)
                    {
                        segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                              new Vector2(endPoint.x + distance, endPoint.y + bendOffset)));
                    }
                    else
                    {
                        // Add line for width to positive Horizontal side of line.
                        segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                              new Vector2(endPoint.x + distance, endPoint.y)));
                    }

                    // Step width
                    widthAdded++;

                    // Add line for width to other side of line
                    if (widthAdded < sizeOfHallways) // recheck size
                    {
                        if (bendInNorthEast)
                        {
                            segments.Add(new Line(new Vector2(startPoint.x + distance, startPoint.y),
                                                  new Vector2(endPoint.x + distance, endPoint.y - bendOffset)));
                        }
                        else
                        {
                            segments.Add(new Line(new Vector2(startPoint.x - distance, startPoint.y),
                                                  new Vector2(endPoint.x - distance, endPoint.y)));
                        }
                    }
                }

                // Step width.
                widthAdded++;
                // Step distance.
                distance++;
            }

            return segments;
        }
    }
}