// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace PolygonClipper;

/// <summary>
/// Implements a robust algorithm for performing boolean operations on polygons.
/// </summary>
/// <remarks>
/// <para>
/// This class is responsible for computing boolean operations such as
/// intersection, union, difference, and XOR between two polygons. The algorithm
/// uses a sweep line approach with an event queue to process polygon segments
/// efficiently. It also handles special cases, including overlapping edges,
/// trivial operations (e.g., non-overlapping polygons), and edge intersections.
/// </para>
/// <para>The main workflow is divided into the following stages:</para>
/// <list type="number">
/// <item><description>Preprocessing: Handles trivial operations and prepares segments for processing.</description></item>
/// <item><description>Sweeping: Processes events using a priority queue, handling segment insertions and removals.</description></item>
/// <item><description>Connecting edges: Constructs the resulting polygon by connecting valid segments.</description></item>
/// </list>
/// </remarks>
public class PolygonClipper
{
    private readonly Polygon subject;
    private readonly Polygon clipping;
    private Polygon result;
    private readonly BooleanOperation operation;

    /// <summary>
    /// The event queue (sorted events to be processed)
    /// </summary>
    private readonly StablePriorityQueue<SweepEvent> eventQueue;

    /// <summary>
    /// To compare events.
    /// </summary>
    private readonly SweepEventComparer sweepEventComparer;

    /// <summary>
    /// The sorted events (sorted events to be processed)
    /// </summary>
    private readonly List<SweepEvent> sortedEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonClipper"/> class.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <param name="result">The result polygon.</param>
    /// <param name="operation">The operation type.</param>
    public PolygonClipper(Polygon subject, Polygon clip, Polygon result, BooleanOperation operation)
    {
        this.subject = subject;
        this.clipping = clip;
        this.result = result;
        this.operation = operation;
        this.sweepEventComparer = new();
        this.eventQueue = new(this.sweepEventComparer);
        this.sortedEvents = new();
    }

    /// <summary>
    /// Computes the intersection of two polygons. The resulting polygon contains the regions that are common to both input polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <returns>A new <see cref="Polygon"/> representing the intersection of the two polygons.</returns>
    public static Polygon Intersection(Polygon subject, Polygon clip)
    {
        Polygon result = new();
        PolygonClipper clipper = new(subject, clip, result, BooleanOperation.Intersection);
        clipper.Run();
        return result;
    }

    /// <summary>
    /// Computes the union of two polygons. The resulting polygon contains the combined regions of the two input polygons.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <returns>A new <see cref="Polygon"/> representing the union of the two polygons.</returns>
    public static Polygon Union(Polygon subject, Polygon clip)
    {
        Polygon result = new();
        PolygonClipper clipper = new(subject, clip, result, BooleanOperation.Union);
        clipper.Run();
        return result;
    }

    /// <summary>
    /// Computes the difference of two polygons. The resulting polygon contains the regions of the subject polygon
    /// that are not shared with the clipping polygon.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <returns>A new <see cref="Polygon"/> representing the difference between the two polygons.</returns>
    public static Polygon Difference(Polygon subject, Polygon clip)
    {
        Polygon result = new();
        PolygonClipper clipper = new(subject, clip, result, BooleanOperation.Difference);
        clipper.Run();
        return result;
    }

    /// <summary>
    /// Computes the symmetric difference (XOR) of two polygons. The resulting polygon contains the regions that belong
    /// to either one of the input polygons but not to their intersection.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clipping polygon.</param>
    /// <returns>A new <see cref="Polygon"/> representing the symmetric difference of the two polygons.</returns>
    public static Polygon Xor(Polygon subject, Polygon clip)
    {
        Polygon result = new();
        PolygonClipper clipper = new(subject, clip, result, BooleanOperation.Xor);
        clipper.Run();
        return result;
    }

    /// <summary>
    /// Executes the boolean operation using the sweep line algorithm.
    /// </summary>
    public void Run()
    {
        // Compute bounding boxes for optimization steps 1 and 2
        Box2 subjectBB = this.subject.BBox();
        Box2 clippingBB = this.clipping.BBox();

        // Check for trivial cases that can be resolved without sweeping
        if (this.TrivialOperation(subjectBB, clippingBB))
        {
            return;
        }

        // Process all segments in the subject polygon
        int contourId = 0;
        for (int i = 0; i < this.subject.NContours; i++)
        {
            Contour contour = this.subject.Contour(i);
            contourId++;
            for (int j = 0; j < contour.NVertices; j++)
            {
                this.ProcessSegment(contourId, contour.Segment(j), PolygonType.Subject);
            }
        }

        // Process all segments in the clipping polygon
        for (int i = 0; i < this.clipping.NContours; i++)
        {
            Contour contour = this.clipping.Contour(i);
            contourId++;
            for (int j = 0; j < contour.NVertices; j++)
            {
                this.ProcessSegment(contourId, contour.Segment(j), PolygonType.Clipping);
            }
        }

        // Sweep line algorithm: process events in the priority queue
        int added = 0;
        StatusLine sweepLine = new();
        float minMaxX = MathF.Min(subjectBB.XMax, clippingBB.XMax);

        SweepEvent se;
        SweepEvent prev;
        SweepEvent next;
        while (this.eventQueue.Count > 0)
        {
            se = this.eventQueue.Dequeue();
            added++;

            // if (added is 15 or 16 or 24)
            // {
            //    Debug.WriteLine("Event Added: " + se.Point);
            // }

            // Optimization: skip further processing if intersection is impossible
            if ((this.operation == BooleanOperation.Intersection && se.Point.X > minMaxX) ||
                (this.operation == BooleanOperation.Difference && se.Point.X > subjectBB.XMax))
            {
                this.ConnectEdges();
                return;
            }

            this.sortedEvents.Add(se);

            if (se.Left)
            {
                // Insert the event into the status line and get neighbors
                sweepLine.Add(se);
                next = sweepLine.GetNext(se);
                prev = sweepLine.GetPrevious(se);

                // Compute fields for the current event
                this.ComputeFields(se, prev);

                // Check intersection with the next neighbor
                if (next != null)
                {
                    // Check intersection with the next neighbor
                    if (this.PossibleIntersection(se, next) == 2)
                    {
                        this.ComputeFields(se, prev);
                        this.ComputeFields(next, se);
                    }
                }

                // Check intersection with the previous neighbor
                if (prev != null)
                {
                    // Check intersection with the previous neighbor
                    if (this.PossibleIntersection(prev, se) == 2)
                    {
                        SweepEvent prevprev = sweepLine.GetPrevious(prev);
                        this.ComputeFields(prev, prevprev);
                        this.ComputeFields(se, prev);
                    }
                }
            }
            else
            {
                // Remove the event from the status line
                se = se.OtherEvent;
                next = prev = sweepLine.Find(se);

                if (prev != null && next != null)
                {
                    prev = sweepLine.GetPrevious(prev);
                    next = sweepLine.GetNext(next);
                    sweepLine.Remove(se);

                    // Check intersection between neighbors
                    if (next != null && prev != null)
                    {
                        // Shift `next` to account for the removal
                        this.PossibleIntersection(prev, next);
                    }
                }
            }

            //if (added is 1)
            //{
            //    Debug.WriteLine("Event Added: " + se.Point);
            //}
        }

        // Connect edges after processing all events
        this.ConnectEdges();
    }

    /// <summary>
    /// Checks for trivial cases in a boolean operation where the result can be determined
    /// without further processing.
    /// </summary>
    /// <param name="subjectBB">The bounding box of the subject polygon.</param>
    /// <param name="clippingBB">The bounding box of the clipping polygon.</param>
    /// <returns>
    /// <see langword="true"/> if the operation results in a trivial case and the result is set;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method performs two tests:
    /// <list type="number">
    /// <item>
    /// <description>Test 1: If either the subject or clipping polygon has zero contours, the result can
    /// be determined immediately based on the operation type.</description>
    /// </item>
    /// <item>
    /// <description>Test 2: If the bounding boxes of the subject and clipping polygons do not overlap,
    /// the result can be determined based on the operation type.</description>
    /// </item>
    /// </list>
    /// </remarks>
    private bool TrivialOperation(Box2 subjectBB, Box2 clippingBB)
    {
        // Test 1 for trivial result case.
        if (this.subject.NContours * this.clipping.NContours == 0)
        {
            if (this.operation == BooleanOperation.Difference)
            {
                this.result = this.subject;
            }

            if (this.operation is BooleanOperation.Union or BooleanOperation.Xor)
            {
                this.result = this.subject.NContours == 0 ? this.clipping : this.subject;
            }

            return true;
        }

        // Test 2 for trivial result case.
        if (subjectBB.XMin > clippingBB.XMax || clippingBB.XMin > subjectBB.XMax ||
            subjectBB.YMin > clippingBB.YMax || clippingBB.YMin > subjectBB.YMax)
        {
            // The bounding boxes do not overlap
            if (this.operation == BooleanOperation.Difference)
            {
                this.result = this.subject;
            }

            if (this.operation is BooleanOperation.Union or BooleanOperation.Xor)
            {
                this.result = this.subject;
                this.result.Join(this.clipping);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Processes a segment by generating sweep events for its endpoints and adding them to the event queue.
    /// </summary>
    /// <param name="contourId">The identifier of the contour to which the segment belongs.</param>
    /// <param name="s">The segment to process.</param>
    /// <param name="pt">The polygon type to which the segment belongs.</param>
    private void ProcessSegment(int contourId, Segment s, PolygonType pt)
    {
        if (s.Source == s.Target)
        {
            // Skip degenerate zero-length segments.
            return;
        }

        // Create sweep events for the endpoints of the segment
        SweepEvent e1 = new(s.Source, true, pt);
        SweepEvent e2 = new(s.Target, true, e1, pt);
        e1.OtherEvent = e2;
        e1.ContourId = e2.ContourId = contourId;

        // Determine which endpoint is the left endpoint
        if (s.Min == s.Source)
        {
            e2.Left = false;
        }
        else if (s.Min == s.Target)
        {
            e1.Left = false;
        }
        else
        {
            // As a fallback, use the comparator for floating-point precision issues
            if (this.sweepEventComparer.Compare(e1, e2) < 0)
            {
                e2.Left = false;
            }
            else
            {
                e1.Left = false;
            }
        }

        // Add the events to the event queue
        this.eventQueue.Enqueue(e1);
        this.eventQueue.Enqueue(e2);
    }

    /// <summary>
    /// Computes fields for a given sweep event.
    /// </summary>
    /// <param name="le">The sweep event to compute fields for.</param>
    /// <param name="prev">The index of the previous event in the status line.</param>
    private void ComputeFields(SweepEvent le, SweepEvent prev)
    {
        // Compute inOut and otherInOut fields
        if (prev == null)
        {
            le.InOut = false;
            le.OtherInOut = true;
        }
        else if (le.PolygonType == prev.PolygonType)
        {
            // Previous line segment in sl belongs to the same polygon that "se" belongs to.
            le.InOut = !prev.InOut;
            le.OtherInOut = prev.OtherInOut;
        }
        else
        {
            // Previous line segment in sl belongs to a different polygon that "se" belongs to.
            le.InOut = !prev.OtherInOut;
            le.OtherInOut = prev.Vertical() ? !prev.InOut : prev.InOut;
        }

        // Compute PrevInResult field
        if (prev != null)
        {
            le.PrevInResult = (!this.InResult(prev) || prev.Vertical())
                ? prev.PrevInResult
                : prev;
        }

        // Check if the line segment belongs to the Boolean operation
        le.InResult = this.InResult(le);
    }

    /// <summary>
    /// Determines if the given sweep event belongs to the result of the boolean operation.
    /// </summary>
    /// <param name="le">The sweep event to check.</param>
    /// <returns><see langword="true"/> if the event belongs to the result; otherwise, <see langword="false"/>.</returns>
    private bool InResult(SweepEvent le)
        => le.EdgeType switch
        {
            EdgeType.Normal => this.operation switch
            {
                BooleanOperation.Intersection => !le.OtherInOut,
                BooleanOperation.Union => le.OtherInOut,
                BooleanOperation.Difference => (le.OtherInOut && le.PolygonType == PolygonType.Subject) ||
                                            (!le.OtherInOut && le.PolygonType == PolygonType.Clipping),
                BooleanOperation.Xor => true,
                _ => false,
            },
            EdgeType.NonContributing => false,
            EdgeType.SameTransition => this.operation is BooleanOperation.Intersection or BooleanOperation.Union,
            EdgeType.DifferentTransition => this.operation == BooleanOperation.Difference,
            _ => false,
        };

    /// <summary>
    /// Determines the possible intersection of two sweep line segments.
    /// </summary>
    /// <param name="le1">The first sweep event representing a line segment.</param>
    /// <param name="le2">The second sweep event representing a line segment.</param>
    /// <returns>
    /// An integer indicating the result of the intersection:
    /// <list type="bullet">
    /// <item><description>0 if no intersection or trivial intersection at endpoints.</description></item>
    /// <item><description>1 if the segments intersect at a single point.</description></item>
    /// <item><description>2 if the segments overlap and share a left endpoint.</description></item>
    /// <item><description>3 if the segments partially overlap or one includes the other.</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the line segments overlap but belong to the same polygon.
    /// </exception>
    private int PossibleIntersection(SweepEvent le1, SweepEvent le2)
    {
        // Point intersections
        int nIntersections = PolygonUtilities.FindIntersection(
            le1.Segment(),
            le2.Segment(),
            out Vector2 ip1,
            out Vector2 _); // Currently unused but could be used to detect collinear overlapping segments

        if (nIntersections == 0)
        {
            // No intersection
            return 0;
        }

        if (nIntersections == 1 &&
            (le1.Point == le2.Point || le1.OtherEvent.Point == le2.OtherEvent.Point))
        {
            // Line segments intersect at an endpoint of both line segments
            return 0;
        }

        if (nIntersections == 2 && le1.PolygonType == le2.PolygonType)
        {
            // Line segments overlap but belong to the same polygon
            return 0;
        }

        // The line segments associated with le1 and le2 intersect
        if (nIntersections == 1)
        {
            // If the intersection point is not an endpoint of le1 segment.
            if (le1.Point != ip1 && le1.OtherEvent.Point != ip1)
            {
                this.DivideSegment(le1, ip1);
            }

            // If the intersection point is not an endpoint of le2 segment.
            if (le2.Point != ip1 && le2.OtherEvent.Point != ip1)
            {
                this.DivideSegment(le2, ip1);
            }

            return 1;
        }

        // The line segments associated with le1 and le2 overlap
        List<SweepEvent> sortedEvents = new();
        if (le1.Point == le2.Point)
        {
            sortedEvents.Add(null);
        }
        else if (this.sweepEventComparer.Compare(le1, le2) > 0)
        {
            sortedEvents.Add(le2);
            sortedEvents.Add(le1);
        }
        else
        {
            sortedEvents.Add(le1);
            sortedEvents.Add(le2);
        }

        if (le1.OtherEvent.Point == le2.OtherEvent.Point)
        {
            sortedEvents.Add(null);
        }
        else if (this.sweepEventComparer.Compare(le1.OtherEvent, le2.OtherEvent) > 0)
        {
            sortedEvents.Add(le2.OtherEvent);
            sortedEvents.Add(le1.OtherEvent);
        }
        else
        {
            sortedEvents.Add(le1.OtherEvent);
            sortedEvents.Add(le2.OtherEvent);
        }

        if (sortedEvents.Count == 2 || (sortedEvents.Count == 3 && sortedEvents[2] != null))
        {
            // Both line segments are equal or share the left endpoint
            le1.EdgeType = EdgeType.NonContributing;
            le2.EdgeType = (le1.InOut == le2.InOut) ? EdgeType.SameTransition : EdgeType.DifferentTransition;
            if (sortedEvents.Count == 3)
            {
                this.DivideSegment(sortedEvents[2].OtherEvent, sortedEvents[1].Point);
            }

            return 2;
        }

        if (sortedEvents.Count == 3)
        {
            // The line segments share the right endpoint
            this.DivideSegment(sortedEvents[0], sortedEvents[1].Point);
            return 3;
        }

        if (sortedEvents[0] != sortedEvents[3].OtherEvent)
        {
            // No line segment includes the other one entirely
            this.DivideSegment(sortedEvents[0], sortedEvents[1].Point);
            this.DivideSegment(sortedEvents[1], sortedEvents[2].Point);
            return 3;
        }

        // One line segment includes the other one
        this.DivideSegment(sortedEvents[0], sortedEvents[1].Point);
        this.DivideSegment(sortedEvents[3].OtherEvent, sortedEvents[2].Point);
        return 3;
    }

    /// <summary>
    /// Divides the given segment at the specified point, creating two new segments.
    /// </summary>
    /// <param name="le">The left event representing the segment to divide.</param>
    /// <param name="p">The point at which to divide the segment.</param>
    private void DivideSegment(SweepEvent le, Vector2 p)
    {
        // Create the right event for the left segment (result of division)
        SweepEvent r = new(p, false, le, le.PolygonType);

        // Create the left event for the right segment (result of division)
        SweepEvent l = new(p, true, le.OtherEvent, le.PolygonType);

        // Assign the same contour id to the new events for sorting.
        r.ContourId = l.ContourId = le.ContourId;

        // Avoid rounding error: ensure the left event is processed before the right event
        if (this.sweepEventComparer.Compare(l, le.OtherEvent) > 0)
        {
            Debug.WriteLine("Rounding error detected: Adjusting left/right flags for event ordering.");
            le.OtherEvent.Left = true;
            l.Left = false;
        }

        if (this.sweepEventComparer.Compare(le, r) > 0)
        {
            Debug.WriteLine("Rounding error detected: Event ordering issue for right event.");
        }

        // Update references to maintain correct linkage
        le.OtherEvent.OtherEvent = l;
        le.OtherEvent = r;

        // Add the new events to the event queue
        this.eventQueue.Enqueue(l);
        this.eventQueue.Enqueue(r);
    }

    /// <summary>
    /// Connects edges in the result polygon by processing the sweep events
    /// and constructing contours for the final result.
    /// </summary>
    private void ConnectEdges()
    {
        // Copy the events in the result polygon to resultEvents list
        List<SweepEvent> resultEvents = new(this.sortedEvents.Count);
        for (int i = 0; i < this.sortedEvents.Count; i++)
        {
            SweepEvent se = this.sortedEvents[i];
            if ((se.Left && se.InResult) || (!se.Left && se.OtherEvent.InResult))
            {
                resultEvents.Add(se);
            }
        }

        // Due to overlapping edges, the resultEvents list may not be completely sorted
        bool sorted = false;
        while (!sorted)
        {
            sorted = true;
            for (int i = 0; i < resultEvents.Count; i++)
            {
                // Positive means "out of order"
                if ((i + 1 < resultEvents.Count) && this.sweepEventComparer.Compare(resultEvents[i], resultEvents[i + 1]) == 1)
                {
                    (resultEvents[i], resultEvents[i + 1]) = (resultEvents[i + 1], resultEvents[i]);
                    sorted = false;
                }
            }
        }

        // Assign positions to events and adjust for right events
        for (int i = 0; i < resultEvents.Count; i++)
        {
            resultEvents[i].Pos = i;
            if (!resultEvents[i].Left)
            {
                (resultEvents[i].Pos, resultEvents[i].OtherEvent.Pos) = (resultEvents[i].OtherEvent.Pos, resultEvents[i].Pos);
            }
        }

        Span<bool> processed = new bool[resultEvents.Count];
        Span<int> depth = new int[resultEvents.Count];
        Span<int> holeOf = new int[resultEvents.Count];
        holeOf.Fill(-1);

        for (int i = 0; i < resultEvents.Count; i++)
        {
            if (processed[i])
            {
                continue;
            }

            Contour contour = new();
            this.result.Push(contour);
            int contourId = this.result.NContours - 1;

            if (resultEvents[i].PrevInResult != null)
            {
                int lowerContourId = resultEvents[i].PrevInResult.ContourId;
                if (!resultEvents[i].PrevInResult.ResultInOut)
                {
                    this.result[lowerContourId].AddHole(contourId);
                    holeOf[contourId] = lowerContourId;
                    depth[contourId] = depth[lowerContourId] + 1;
                    contour.External = false;
                }
                else if (!this.result[lowerContourId].External)
                {
                    this.result[holeOf[lowerContourId]].AddHole(contourId);
                    holeOf[contourId] = holeOf[lowerContourId];
                    depth[contourId] = depth[lowerContourId];
                    contour.External = false;
                }
            }

            int pos = i;
            int originalPos = i;
            Vector2 initial = resultEvents[i].Point;
            contour.Add(initial);

            do
            {
                processed[pos] = true;
                if (resultEvents[pos].Left)
                {
                    resultEvents[pos].ResultInOut = false;
                    resultEvents[pos].ContourId = contourId;
                }
                else
                {
                    resultEvents[pos].OtherEvent.ResultInOut = true;
                    resultEvents[pos].OtherEvent.ContourId = contourId;
                }

                processed[pos = resultEvents[pos].Pos] = true;
                contour.Add(resultEvents[pos].Point);
                pos = NextPos(pos, resultEvents, processed, originalPos);
            }
            while (pos != originalPos && pos < resultEvents.Count);

            processed[pos] = processed[resultEvents[pos].Pos] = true;
            resultEvents[pos].OtherEvent.ResultInOut = true;
            resultEvents[pos].OtherEvent.ContourId = contourId;
        }
    }

    /// <summary>
    /// Finds the next unprocessed position in the result events, either forward or backward,
    /// starting from the given position.
    /// </summary>
    /// <param name="pos">The current position in the result events.</param>
    /// <param name="resultEvents">The list of sweep events representing result segments.</param>
    /// <param name="processed">A list indicating whether each event at the corresponding index has been processed.</param>
    /// <param name="originalPos">The original position to return if no unprocessed event is found.</param>
    /// <returns>The index of the next unprocessed position.</returns>
    /// <remarks>
    /// This method searches forward from the current position until it finds an unprocessed event with
    /// a different point or reaches the end of the list. If no such event is found, it searches backward
    /// until it finds an unprocessed event.
    /// </remarks>
    private static int NextPos(
        int pos,
        List<SweepEvent> resultEvents,
        ReadOnlySpan<bool> processed,
        int originalPos)
    {
        int newPos = pos + 1;

        // Search forward for the next unprocessed event with a different point
        while (newPos < resultEvents.Count && resultEvents[newPos].Point == resultEvents[pos].Point)
        {
            if (!processed[newPos])
            {
                return newPos;
            }

            newPos++;
        }

        // If not found, search backward for an unprocessed event
        newPos = pos - 1;
        while (newPos > originalPos && processed[newPos])
        {
            newPos--;
        }

        return newPos;
    }
}
