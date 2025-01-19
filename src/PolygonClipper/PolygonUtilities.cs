// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
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
    /// <returns>The <see cref="float"/> area.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignedArea(Vector2 p0, Vector2 p1, Vector2 p2)
        => ((p0.X - p2.X) * (p1.Y - p2.Y)) - ((p1.X - p2.X) * (p0.Y - p2.Y));

    /// <summary>
    /// Finds the intersection between two intervals [u0, u1] and [v0, v1].
    /// </summary>
    /// <param name="u0">The start of the first interval.</param>
    /// <param name="u1">The end of the first interval.</param>
    /// <param name="v0">The start of the second interval.</param>
    /// <param name="v1">The end of the second interval.</param>
    /// <param name="w">
    /// An array to store the result of the intersection.
    /// - If the intersection is a single point, the first element of the array will contain the intersection point.
    /// - If the intersection is an interval, the first two elements will contain the start and end of the intersection range.
    /// </param>
    /// <returns>
    /// An <see cref="int"/> indicating the type of intersection:
    /// - Returns 0 if there is no intersection.
    /// - Returns 1 if the intersection is a single point.
    /// - Returns 2 if the intersection is an interval.
    /// </returns>
    public static int FindIntersection(float u0, float u1, float v0, float v1, Span<float> w)
    {
        if ((u1 < v0) || (u0 > v1))
        {
            return 0; // No intersection
        }

        if (u1 > v0)
        {
            if (u0 < v1)
            {
                // There is an overlapping range
                w[0] = (u0 < v0) ? v0 : u0;
                w[1] = (u1 > v1) ? v1 : u1;
                return 2; // Two endpoints defining the intersection range
            }

            // u0 == v1
            w[0] = u0;

            return 1; // Single point intersection
        }

        // u1 == v0
        w[0] = u1;

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
    public static int FindIntersection(Segment seg0, Segment seg1, out Vector2 pi0, out Vector2 pi1)
    {
        pi0 = default;
        pi1 = default;

        Vector2 p0 = seg0.Source;
        Vector2 d0 = seg0.Target - p0;
        Vector2 p1 = seg1.Source;
        Vector2 d1 = seg1.Target - p1;

        const float sqrEpsilon = 0.00001F; // Threshold for floating-point precision
        Vector2 e = p1 - p0;
        float kross = (d0.X * d1.Y) - (d0.Y * d1.X);
        float sqrKross = kross * kross;
        float sqrLen0 = (d0.X * d0.X) + (d0.Y * d0.Y);
        float sqrLen1 = (d1.X * d1.X) + (d1.Y * d1.Y);

        if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1)
        {
            // Lines of the segments are not parallel
            float s = ((e.X * d1.Y) - (e.Y * d1.X)) / kross;
            if (s is < 0 or > 1)
            {
                return 0;
            }

            float t = ((e.X * d0.Y) - (e.Y * d0.X)) / kross;
            if (t is < 0 or > 1)
            {
                return 0;
            }

            // Intersection of lines is a point on each segment
            pi0 = p0 + (s * d0);
            SnapToSegmentEndpoint(ref pi0, seg0);
            SnapToSegmentEndpoint(ref pi0, seg1);
            return 1;
        }

        // Lines of the segments are parallel
        float sqrLenE = (e.X * e.X) + (e.Y * e.Y);
        kross = (e.X * d0.Y) - (e.Y * d0.X);
        sqrKross = kross * kross;
        if (sqrKross > sqrEpsilon * sqrLen0 * sqrLenE)
        {
            // Lines of the segments are different
            return 0;
        }

        // Lines of the segments are the same. Need to test for overlap of segments.
        float s0 = ((d0.X * e.X) + (d0.Y * e.Y)) / sqrLen0; // so = Dot (D0, E) * sqrLen0
        float s1 = s0 + (((d0.X * d1.X) + (d0.Y * d1.Y)) / sqrLen0); // s1 = s0 + Dot (D0, D1) * sqrLen0
        float smin = Math.Min(s0, s1);
        float smax = Math.Max(s0, s1);
        Span<float> w = stackalloc float[2];
        int imax = FindIntersection(0.0f, 1.0f, smin, smax, w);

        if (imax > 0)
        {
            pi0 = new Vector2(p0.X + (w[0] * d0.X), p0.Y + (w[0] * d0.Y));
            SnapToSegmentEndpoint(ref pi0, seg0);
            SnapToSegmentEndpoint(ref pi0, seg1);
            if (imax > 1)
            {
                pi1 = new Vector2(p0.X + (w[1] * d0.X), p0.Y + (w[1] * d0.Y));
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
    private static void SnapToSegmentEndpoint(ref Vector2 point, Segment segment)
    {
        const float threshold = 0.00001F;

        if (Vector2.Distance(point, segment.Source) < threshold)
        {
            point = segment.Source;
        }
        else if (Vector2.Distance(point, segment.Target) < threshold)
        {
            point = segment.Target;
        }
    }
}
