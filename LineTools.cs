using System.Collections.Generic;
using UnityEngine;

public static class LineTools
{
    /// <summary>
    /// Changes grid location to be on a map that's origin is 0,0 rather then world space.
    /// </summary>
    /// <param name="lines">The lines to offset.</param>
    /// <param name="lowestWorldPoint">Lowest point in world location on map.</param>
    /// <returns></returns>
    public static List<Line> TranslateWorldToGridLocation(List<Line> lines, Point lowestWorldPoint)
    {
        List<Line> offsetLines = new List<Line>();

        foreach (Line line in lines)
        {
            // Snap line locations to grid space (ints)
            int p0x = Mathf.RoundToInt(line.p0.x) - lowestWorldPoint.X;
            int p0y = Mathf.RoundToInt(line.p0.y) - lowestWorldPoint.Y;

            int p1x = Mathf.RoundToInt(line.p1.x) - lowestWorldPoint.X;
            int p1y = Mathf.RoundToInt(line.p1.y) - lowestWorldPoint.Y;

            offsetLines.Add(new Line(new Vector2(p0x, p0y), new Vector2(p1x, p1y)));
        }

        return offsetLines;
    }
}
