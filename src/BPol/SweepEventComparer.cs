// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections;
using System.Collections.Generic;

namespace BPol;

/// <summary>
/// Compares two <see cref="SweepEvent"/> instances for sorting in the event queue.
/// </summary>
internal class SweepEventComparer : IComparer<SweepEvent>, IComparer
{
    /// <inheritdoc/>
    public int Compare(SweepEvent x, SweepEvent y)
    {
        // If the events are the same, return 0 (no order difference)
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        // Compare by x-coordinate
        if (x.Point.X > y.Point.X)
        {
            return 1;
        }

        if (x.Point.X < y.Point.X)
        {
            return -1;
        }

        // Different points, but same x-coordinate.
        // The event with lower y-coordinate is processed first
        if (x.Point.Y != y.Point.Y)
        {
            return x.Point.Y > y.Point.Y ? 1 : -1;
        }

        // Same point, but one is a left endpoint and the other a right endpoint.
        // The right endpoint is processed first
        if (x.Left != y.Left)
        {
            return x.Left ? 1 : -1;
        }

        // Same point, both events are left endpoints or both are right endpoints.
        // Compare by signed area if the segments are not collinear
        float area = PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.OtherEvent.Point);
        if (area != 0F)
        {
            // The event associate to the bottom segment is processed first
            return x.Above(y.OtherEvent.Point) ? 1 : -1;
        }

        // Compare by polygon ID (higher ID is processed first)
        return x.ContourId.CompareTo(y.ContourId);
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
