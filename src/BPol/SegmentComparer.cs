// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BPol;

/// <summary>
/// Allows the comparison of segments for sorting.
/// </summary>
public sealed class SegmentComparer : IComparer<SweepEvent>, IComparer
{
    /// <inheritdoc/>
    public int Compare(SweepEvent x, SweepEvent y)
    {
        // Reference equals?
        if (x == y)
        {
            return 0;
        }

        if (PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.Point) != 0F
            || PolygonUtilities.SignedArea(x.Point, x.OtherEvent.Point, y.OtherEvent.Point) != 0)
        {
            // Segments are not collinear
            // If they share their left endpoint use the right endpoint to sort.
            if (x.Point == y.Point)
            {
                return x.Below(y.OtherEvent.Point) ? -1 : 1;
            }

            // Different points
            // has the segment associated to 'x' been sorted in evp before
            // the segment associated to 'y'?
            if (CompareEvents(x, y) < 0)
            {
                return x.Below(y.Point) ? -1 : 1;
            }

            // The segment associated to 'y' has been sorted in evp before the segment associated to 'x'
            return y.Above(x.Point) ? 1 : -1;
        }

        // Segments are collinear. Just a consistent criterion is used
        if (x.Point == y.Point)
        {
            // Since they are geometrically identical, we impose a consistent but arbitrary order
            // by comparing their unique IDs. This ensures stable, deterministic sorting without
            // treating them as equal, similar to pointer comparison in C++.
            return x.Id < y.Id ? -1 : 1;
        }

        return CompareEvents(x, y);
    }

    /// <summary>
    /// Compares two sweep events.
    /// </summary>
    /// <param name="x">The left hand sweep event.</param>
    /// <param name="y">The right hand sweep event.</param>
    /// <returns>An <see cref="int"/> that indicates the relative values of x and y.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareEvents(SweepEvent x, SweepEvent y)
    {
        if (x.Point.X < y.Point.X)
        {
            // Different x coordinate
            return -1;
        }

        if (y.Point.X < x.Point.X)
        {
            // Different x coordinate
            return 1;
        }

        if (x.Point != y.Point)
        {
            // Different points, but same x coordinate.
            // The event with lower y coordinate is processed first
            return x.Point.Y < y.Point.Y ? -1 : 1;
        }

        if (x.Left != y.Left)
        {
            // Same point, but one is a left endpoint and the other
            // a right endpoint. The right endpoint is processed first
            return x.Left ? 1 : -1;
        }

        // Same point, both events are left endpoints or both are right endpoints.
        // The event associate to the bottom segment is processed first
        return x.Below(y.OtherEvent.Point) ? -1 : 1;
    }

    /// <inheritdoc/>
    public int Compare(object x, object y)
    {
        if (x == y)
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

        if (x is SweepEvent a && y is SweepEvent b)
        {
            return this.Compare(a, b);
        }

        throw new ArgumentException(string.Empty, nameof(x));
    }
}
