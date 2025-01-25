// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Generic;
using Xunit;

namespace PolygonClipper.Tests;

public class SweepEventComparerTests
{
    private readonly SweepEventComparer comparer = new();

    [Fact]
    public void Queue_ShouldProcessLeastByX_SweepEventFirst()
    {
        PriorityQueue<SweepEvent, SweepEvent> queue = new(this.comparer);
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(.5F, .5F), true);

        queue.Enqueue(e1, e1);
        queue.Enqueue(e2, e2);

        Assert.Equal(e1, queue.Dequeue());
        Assert.Equal(e2, queue.Dequeue());
    }

    [Fact]
    public void Queue_ShouldProcessLeastByY_SweepEventFirst()
    {
        PriorityQueue<SweepEvent, SweepEvent> queue = new(this.comparer);
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(0, .5F), true);

        queue.Enqueue(e1, e1);
        queue.Enqueue(e2, e2);

        Assert.Equal(e1, queue.Dequeue());
        Assert.Equal(e2, queue.Dequeue());
    }

    [Fact]
    public void Queue_ShouldPopLeastByLeftProp_SweepEventFirst()
    {
        PriorityQueue<SweepEvent, SweepEvent> queue = new(this.comparer);
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(0, 0), false);

        queue.Enqueue(e1, e1);
        queue.Enqueue(e2, e2);

        Assert.Equal(e2, queue.Dequeue());
        Assert.Equal(e1, queue.Dequeue());
    }

    [Fact]
    public void SweepEventComparison_XCoordinates()
    {
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(.5F, .5F), true);

        SweepEventComparer comparer = new();
        Assert.True(comparer.Compare(e1, e2) < 0);
        Assert.True(comparer.Compare(e2, e1) > 0);
    }

    [Fact]
    public void SweepEventComparison_YCoordinates()
    {
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(0, .5F), true);

        SweepEventComparer comparer = new();
        Assert.True(comparer.Compare(e1, e2) < 0);
        Assert.True(comparer.Compare(e2, e1) > 0);
    }

    [Fact]
    public void SweepEventComparison_NotLeftFirst()
    {
        SweepEvent e1 = new(new Vertex(0, 0), true);
        SweepEvent e2 = new(new Vertex(0, 0), false);

        SweepEventComparer comparer = new();
        Assert.True(comparer.Compare(e1, e2) > 0);
        Assert.True(comparer.Compare(e2, e1) < 0);
    }

    [Fact]
    public void SweepEventComparison_SharedStartPoint_NotCollinearEdges()
    {
        SweepEvent e1 = new(new Vertex(0, 0), true, new SweepEvent(new Vertex(1, 1), false));
        SweepEvent e2 = new(new Vertex(0, 0), true, new SweepEvent(new Vertex(2, 3), false));

        SweepEventComparer comparer = new();
        Assert.True(comparer.Compare(e1, e2) < 0);
        Assert.True(comparer.Compare(e2, e1) > 0);
    }

    [Fact]
    public void SweepEventComparison_CollinearEdges()
    {
        SweepEvent e1 = new(new Vertex(0, 0), true, new SweepEvent(new Vertex(1, 1), false), PolygonType.Clipping);
        SweepEvent e2 = new(new Vertex(0, 0), true, new SweepEvent(new Vertex(2, 2), false));

        SweepEventComparer comparer = new();

        // The C++ reference differs from the JavaScript reference that the test is ported from.
        // In the C++ implementation, when comparing two collinear segments with the same start point,
        // the priority is determined by the polygon type. Specifically, edges belonging to the clipping
        // polygon (PolygonType.CLIPPING) are processed after edges belonging to the subject polygon 
        // (PolygonType.SUBJECT). This is achieved by comparing polygon identifiers, where the higher 
        // ID (e.g., CLIPPING > SUBJECT) is processed first.
        //
        // In contrast, the JavaScript implementation uses a reversed priority order for polygon types,
        // prioritizing edges from the subject polygon over the clipping polygon.
        //
        // This test aligns with the C++ behavior, which ensures consistency with the reference source 
        // used for the algorithm's logic.
        Assert.True(comparer.Compare(e1, e2) > 0);
        Assert.True(comparer.Compare(e2, e1) < 0);
    }
}
