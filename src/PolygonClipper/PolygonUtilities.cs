// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Runtime.CompilerServices;

namespace PolygonClipper;

internal static class PolygonUtilities
{
    /// <summary>
    /// Returns the signed area of a triangle.
    /// </summary>
    /// <param name="p0">The first point.</param>
    /// <param name="p1">The second point.</param>
    /// <param name="p2">The third point.</param>
    /// <returns>The <see cref="double"/> area.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SignedArea(Vertex p0, Vertex p1, Vertex p2)
        => ((p0.X - p2.X) * (p1.Y - p2.Y)) - ((p1.X - p2.X) * (p0.Y - p2.Y));

    /// <summary>
    /// Finds the intersection between two intervals [u0, u1] and [v0, v1].
    /// </summary>
    /// <param name="u0">The start of the first interval.</param>
    /// <param name="u1">The end of the first interval.</param>
    /// <param name="v0">The start of the second interval.</param>
    /// <param name="v1">The end of the second interval.</param>
    /// <param name="start">
    /// The start of the intersection interval, or the single intersection point if there is only one.
    /// </param>
    /// <param name="end">
    /// The end of the intersection interval. If the intersection is a single point, this value is undefined.
    /// </param>
    /// <returns>
    /// An <see cref="int"/> indicating the type of intersection:
    /// - Returns 0 if there is no intersection.
    /// - Returns 1 if the intersection is a single point.
    /// - Returns 2 if the intersection is an interval.
    /// </returns>
    public static int FindIntersection(double u0, double u1, double v0, double v1, out double start, out double end)
    {
        start = 0;
        end = 0;

        if ((u1 < v0) || (u0 > v1))
        {
            return 0; // No intersection
        }

        if (u1 > v0)
        {
            if (u0 < v1)
            {
                // There is an overlapping range
                start = (u0 < v0) ? v0 : u0;
                end = (u1 > v1) ? v1 : u1;
                return 2; // Two endpoints defining the intersection range
            }

            // u0 == v1
            start = u0;

            return 1; // Single point intersection
        }

        // u1 == v0
        start = u1;

        return 1; // Single point intersection
    }

    /// <summary>
    /// Finds the intersection of two line segments.
    /// </summary>
    /// <param name="seg0">The first line segment.</param>
    /// <param name="seg1">The second line segment.</param>
    /// <param name="pi0">
    /// The first intersection point (if any). If the segments intersect at a single point, this will contain the intersection point.
    /// If the segments overlap, this will contain the start of the overlapping segment.
    /// </param>
    /// <param name="pi1">
    /// The second intersection point (if any). If the segments overlap, this will contain the end of the overlapping segment.
    /// </param>
    /// <returns>
    /// An <see cref="int"/> indicating the number of intersection points:
    /// - Returns 0 if there is no intersection.
    /// - Returns 1 if the segments intersect at a single point.
    /// - Returns 2 if the segments overlap.
    /// </returns>
    public static int FindIntersection(Segment seg0, Segment seg1, out Vertex pi0, out Vertex pi1)
    {
        pi0 = default;
        pi1 = default;

        Vertex p0 = seg0.Source;
        Vertex p1 = seg1.Source;

        Vertex va = seg0.Target - p0;
        Vertex vb = seg1.Target - p1;

        const double sqrEpsilon = 0.0000001; // Threshold for comparing-point precision
        Vertex e = p1 - p0;
        double kross = Vertex.Cross(va, vb);
        double sqrKross = kross * kross;
        double sqrLenA = Vertex.Dot(va, va);
        // double sqrLen1 = Vertex.Dot(vb, vb);

        if (sqrKross > 0)
        {
            // Lines of the segments are not parallel
            double s = Vertex.Cross(e, vb) / kross;
            if (s is < 0 or > 1)
            {
                return 0;
            }

            double t = Vertex.Cross(e, va) / kross;
            if (t is < 0 or > 1)
            {
                return 0;
            }

            if (s == 0 || s == 1)
            {
                // on an endpoint of line segment a
                pi0 = p0 + (s * va);
                return 1;
            }

            if (t == 0 || t == 1)
            {
                // on an endpoint of line segment b
                pi0 = p1 + (t * vb);
                return 1;
            }

            // Intersection of lines is a point on each segment
            pi0 = p0 + (s * va);
            return 1;
        }

        // Lines of the segments are parallel
        // double sqrLenE = (e.X * e.X) + (e.Y * e.Y);
        kross = Vertex.Cross(e, va);
        sqrKross = kross * kross;
        if (sqrKross > 0)
        {
            // Lines of the segments are different
            return 0;
        }

        // Lines of the segments are the same. Need to test for overlap of segments.
        double s0 = Vertex.Dot(va, e) / sqrLenA; // so = Dot (D0, E) * sqrLen0
        double s1 = s0 + (Vertex.Dot(va, vb) / sqrLenA); // s1 = s0 + Dot (D0, D1) * sqrLen0
        double smin = Math.Min(s0, s1);
        double smax = Math.Max(s0, s1);
        int imax = FindIntersection(0F, 1F, smin, smax, out double w0, out double w1);

        if (imax > 0)
        {
            pi0 = new Vertex(p0.X + (w0 * va.X), p0.Y + (w0 * va.Y));
            SnapToSegmentEndpoint(ref pi0, seg0);
            SnapToSegmentEndpoint(ref pi0, seg1);
            if (imax > 1)
            {
                pi1 = new Vertex(p0.X + (w1 * va.X), p0.Y + (w1 * va.Y));
            }
        }

        return imax;
    }

    /// <summary>
    /// Snaps the point to the nearest endpoint of the segment if it is very close.
    /// </summary>
    /// <param name="point">The point to snap.</param>
    /// <param name="segment">The segment to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SnapToSegmentEndpoint(ref Vertex point, Segment segment)
    {
        const double threshold = 0.00000001;

        if (Vertex.Distance(point, segment.Source) < threshold)
        {
            point = segment.Source;
        }
        else if (Vertex.Distance(point, segment.Target) < threshold)
        {
            point = segment.Target;
        }
    }
}
