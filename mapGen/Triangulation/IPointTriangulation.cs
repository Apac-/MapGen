using System.Collections.Generic;
using UnityEngine;

namespace MapGen
{
    public interface IPointTriangulation
    {
        List<Line> FindConnectingLineSegments(List<Vector2> centerPoints, float percentOfSegmentsAboveMinSpanningTree);
    }
}