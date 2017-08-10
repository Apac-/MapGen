using System.Collections.Generic;
using UnityEngine;

public interface IPointTriangulation
{
    List<Line> FindConnectingLineSegments(List<Vector2> centerPoints, float percentOfSegmentsAboveMinSpanningTree);
}