// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
    public int Compare(SweepEvent x, SweepEvent y)
    {
        // If the events are the same, return 0 (no order difference)
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        // Check if the segments are collinear by comparing their signed areas
        float area1 = PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.Point);
        float area2 = PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.OtherEvent.Point);

        if (area1 != 0F || area2 != 0F)
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
                return y.Above(x.Point) ? 1 : -1;
            }

            // The line segment associated with "y" has been inserted after "x"
            return x.Below(y.Point) ? 1 : -1;
        }

        // Segments are collinear
        if (x.PolygonType != y.PolygonType)
        {
            return x.PolygonType < y.PolygonType ? -1 : 1;
        }

        // Use a consistent ordering criterion for collinear segments with the same id.
        if (x.Point == y.Point)
        {
            return x.Id.CompareTo(y.Id);
        }

        // Fall back to the sweep event comparator for final comparison
        return this.eventComparer.Compare(x, y);
    }

    /// <inheritdoc/>
    public int Compare(object x, object y)
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
