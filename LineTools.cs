using System.Collections.Generic;
using UnityEngine;

public static class LineTools
{
    /// <summary>
    /// Returns hallway lines with new offset positions of a map that has an origin at 0,0.
    /// </summary>
    /// <param name="lines">The lines to offset.</param>
    /// <param name="newOrigin">The lowest point in real space of the current map before zeroing out.</param>
    /// <returns></returns>
    public static List<Line> OffsetLinesToNewMapOrigin(List<Line> lines, Point newOrigin)
    {
        List<Line> offsetLines = new List<Line>();

        foreach (Line line in lines)
        {
            // Snap line locations to grid space (ints)
            int p0x = Mathf.RoundToInt(line.p0.x) - newOrigin.X;
            int p0y = Mathf.RoundToInt(line.p0.y) - newOrigin.Y;

            int p1x = Mathf.RoundToInt(line.p1.x) - newOrigin.X;
            int p1y = Mathf.RoundToInt(line.p1.y) - newOrigin.Y;

            offsetLines.Add(new Line(new Vector2(p0x, p0y), new Vector2(p1x, p1y)));
        }

        return offsetLines;
    }
}
