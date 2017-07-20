using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayGrapher {

    private Delaunay.Voronoi voroni;

    public DelaunayGrapher(List<Vector2> centerPoints)
    {
        List<uint> colors = new List<uint>();
        for (int i = 0; i < centerPoints.Count; i++)
        {
            colors.Add(0);
        }

        voroni = new Delaunay.Voronoi(centerPoints, colors, new Rect(0, 0, 200, 200));
    }

    /// <summary>
    /// Find minium amount of connecting line segments between supplied room center points.
    /// </summary>
    /// <param name="percentOfSegementsAboveMinThreshold">Percent of extra connecting line segments to return. If 0% then only returns min spanning tree.</param>
    /// <returns></returns>
    public List<LineSegment> FindLineSegments(float percentOfSegementsAboveMinThreshold)
    {
        List<LineSegment> dTriangulation = voroni.DelaunayTriangulation();

        List<LineSegment> tree = voroni.SpanningTree(Delaunay.KruskalType.MINIMUM);

        int numOfExtraSegments = Mathf.RoundToInt((float)dTriangulation.Count * percentOfSegementsAboveMinThreshold);

        List<LineSegment> finalSegments = new List<LineSegment>();

        int extraSegCount = 0;

        foreach (LineSegment segment in dTriangulation)
        {
            bool foundInTree = false;

            // always add the min spanning tree segments to final.
            foreach (LineSegment treeSeg in tree) {
                if (segment.p0.Value == treeSeg.p0.Value &&
                    segment.p1.Value == treeSeg.p1.Value) {
                    finalSegments.Add(segment);
                    foundInTree = true;
                    break;
                }
            }

            // Add in the extra segements.
            if (!foundInTree && extraSegCount < numOfExtraSegments) {
                finalSegments.Add(segment);
                extraSegCount++;
            }
        }

        return finalSegments;
    }
}
