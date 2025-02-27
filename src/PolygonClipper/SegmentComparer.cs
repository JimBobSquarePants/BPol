// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace PolygonClipper;

/// <summary>
/// Allows the comparison of segments for sorting.
/// </summary>
internal sealed class SegmentComparer : IComparer<SweepEvent>, IComparer
{
    private readonly SweepEventComparer eventComparer = new();

    /// <inheritdoc/>
    public int Compare(SweepEvent? x, SweepEvent? y)
    {
        // If the events are the same, return 0 (no order difference)
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        // Check if the segments are collinear by comparing their signed areas
        double area1 = PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.Point);
        double area2 = PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.OtherEvent.Point);

        if (area1 != 0 || area2 != 0)
        {
            // Segments are not collinear
            // If they share their left endpoint, use the right endpoint to sort
            if (x.Point == y.Point)
            {
                return x.Below(y.OtherEvent.Point) ? -1 : 1;
            }

            // Different left endpoints: use the y-coordinate to sort if x-coordinates are the same
            if (x.Point.X == y.Point.X)
            {
                return x.Point.Y < y.Point.Y ? -1 : 1;
            }

            // Has the line segment associated to "x" been inserted into the segment after the line
            // segment associated to "y"?
            // Use the sweep event order to determine the comparison
            int compResult = this.eventComparer.Compare(x, y);
            if (compResult == 1)
            {
                return y.Above(x.Point) ? -1 : 1;
            }

            // The line segment associated with "y" has been inserted after "x"
            return x.Below(y.Point) ? -1 : 1;
        }

        // JavaScript comparer is different to C++
        if (x.PolygonType == y.PolygonType) // Same polygon
        {
            Vertex p1 = x.Point;
            Vertex p2 = y.Point;

            if (p1 == p2) // Points are the same
            {
                // Compare the other endpoints of the segments
                p1 = x.OtherEvent.Point;
                p2 = y.OtherEvent.Point;

                if (p1 == p2) // Other endpoints are also the same
                {
                    return 0;
                }

                return x.ContourId > y.ContourId ? 1 : -1;
            }
        }
        else // Segments are collinear but belong to separate polygons
        {
            return x.PolygonType == PolygonType.Subject ? -1 : 1;
        }

        // Fall back to the sweep event comparator for final comparison
        return this.eventComparer.Compare(x, y) == 1 ? 1 : -1;
    }

    /// <inheritdoc/>
    public int Compare(object? x, object? y)
    {
        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        if (x is SweepEvent a && y is SweepEvent b)
        {
            return this.Compare(a, b);
        }

        throw new ArgumentException("Both arguments must be of type SweepEvent.", nameof(x));
    }
}
