// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace PolygonClipper.Tests;
public class SegmentComparerTests
{
    private readonly SegmentComparer segmentComparer = new();

    [Fact]
    public void NotCollinear_SharedLeftPoint_RightPointFirst()
    {
        SortedSet<SweepEvent> tree = new(this.segmentComparer);
        Vector2 pt = new(0, 0);
        SweepEvent se1 = new(pt, true, new SweepEvent(new Vector2(1, 1), false));
        SweepEvent se2 = new(pt, true, new SweepEvent(new Vector2(2, 3), false));

        tree.Add(se1);
        tree.Add(se2);

        Assert.Equal(new Vector2(2, 3), tree.Max.OtherEvent.Point);
        Assert.Equal(new Vector2(1, 1), tree.Min.OtherEvent.Point);
    }

    [Fact]
    public void NotCollinear_DifferentLeftPoint_RightPointYCoordToSort()
    {
        SortedSet<SweepEvent> tree = new(this.segmentComparer);
        SweepEvent se1 = new(new Vector2(0, 1), true, new SweepEvent(new Vector2(1, 1), false));
        SweepEvent se2 = new(new Vector2(0, 2), true, new SweepEvent(new Vector2(2, 3), false));

        tree.Add(se1);
        tree.Add(se2);

        Assert.Equal(new Vector2(1, 1), tree.Min.OtherEvent.Point);
        Assert.Equal(new Vector2(2, 3), tree.Max.OtherEvent.Point);
    }

    [Fact]
    public void NotCollinear_EventsOrderInSweepLine()
    {
        SweepEvent se1 = new(new Vector2(0, 1), true, new SweepEvent(new Vector2(2, 1), false));
        SweepEvent se2 = new(new Vector2(-1, 0), true, new SweepEvent(new Vector2(2, 3), false));

        SweepEvent se3 = new(new Vector2(0, 1), true, new SweepEvent(new Vector2(3, 4), false));
        SweepEvent se4 = new(new Vector2(-1, 0), true, new SweepEvent(new Vector2(3, 1), false));

        SweepEventComparer eventComparer = new();

        Assert.Equal(1, eventComparer.Compare(se1, se2));
        Assert.False(se2.Below(se1.Point));
        Assert.True(se2.Above(se1.Point));

        Assert.Equal(-1, this.segmentComparer.Compare(se1, se2));
        Assert.Equal(1, this.segmentComparer.Compare(se2, se1));

        Assert.Equal(1, eventComparer.Compare(se3, se4));
        Assert.False(se4.Above(se3.Point));
    }

    [Fact]
    public void FirstPointIsBelow()
    {
        SweepEvent se2 = new(new Vector2(0, 1), true, new SweepEvent(new Vector2(2, 1), false));
        SweepEvent se1 = new(new Vector2(-1, 0), true, new SweepEvent(new Vector2(2, 3), false));

        Assert.False(se1.Below(se2.Point));

        Assert.Equal(1, this.segmentComparer.Compare(se1, se2));
    }

    [Fact]
    public void CollinearSegments()
    {
        SweepEvent se1 = new(new Vector2(1, 1), true, new SweepEvent(new Vector2(5, 1), false), PolygonType.SUBJECT);
        SweepEvent se2 = new(new Vector2(2, 1), true, new SweepEvent(new Vector2(3, 1), false), PolygonType.CLIPPING);

        // Assert that the segments belong to different polygons
        Assert.NotEqual(se1.PolygonType, se2.PolygonType);
        Assert.Equal(-1, this.segmentComparer.Compare(se1, se2));
    }

    [Fact]
    public void CollinearSharedLeftPoint()
    {
        // Arrange
        Vector2 pt = new(0, 1);

        SweepEvent se1 = new(pt, true, new SweepEvent(new Vector2(5, 1), false), PolygonType.CLIPPING);
        SweepEvent se2 = new(pt, true, new SweepEvent(new Vector2(3, 1), false), PolygonType.CLIPPING);

        se1.ContourId = 1;
        se2.ContourId = 2;

        // Assert that the segments belong to the same polygon type
        Assert.Equal(se1.PolygonType, se2.PolygonType);

        // Assert that the segments share the same starting point
        Assert.Equal(se1.Point, se2.Point);
        Assert.Equal(-1, this.segmentComparer.Compare(se1, se2));

        // Recreate our items swapping the order and compare again; the order should now be reversed.
        //
        // We compare by unique IDs generated during SweepEvent construction.
        // This differs from the JavaScript version, which uses contourId for comparison, 
        // and from the C++ version, which relies on pointer comparison to determine 
        // a consistent ordering for collinear segments. 
        //
        // In this implementation, the unique ID is generated via a static counter during 
        // the construction of each SweepEvent. This ensures that every event has a distinct
        // identifier, mimicking the pointer-based uniqueness in the C++ version.
        //
        // The order of comparison is reversed when the creation order of the SweepEvent instances is swapped. 
        // This behavior is consistent with the use of unique IDs to establish a deterministic and 
        // consistent order for collinear segments, which may vary based on the sequence in which the
        // instances are created.
        se2 = new(pt, true, new SweepEvent(new Vector2(3, 1), false));
        se1 = new(pt, true, new SweepEvent(new Vector2(5, 1), false));
        Assert.Equal(1, this.segmentComparer.Compare(se1, se2));
    }

    [Fact]
    public void CollinearSamePolygonDifferentLeftPoints()
    {
        SweepEvent se1 = new(new Vector2(1, 1), true, new SweepEvent(new Vector2(5, 1), false), PolygonType.SUBJECT);
        SweepEvent se2 = new(new Vector2(2, 1), true, new SweepEvent(new Vector2(3, 1), false), PolygonType.SUBJECT);

        Assert.Equal(se1.PolygonType, se2.PolygonType);
        Assert.NotEqual(se1.Point, se2.Point);

        Assert.Equal(-1, this.segmentComparer.Compare(se1, se2));
        Assert.Equal(1, this.segmentComparer.Compare(se2, se1));
    }
}
