// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BPol;

/// <summary>
/// Represents a sweep.
/// </summary>
public class SweepEvent
{
    /// <summary>
    /// Static counter to ensure unique IDs
    /// </summary>
    private static long nextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SweepEvent"/> class.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="left">Whether the point is the left (source) endpoint of the segment.</param>
    /// <param name="polygon">The polygon.</param>
    public SweepEvent(Vector2 point, bool left, int polygon)
    {
        this.Point = point;
        this.Left = left;
        this.Polygon = polygon;
        this.Id = Interlocked.Increment(ref nextId); // Ensure thread safety if needed
    }

    /// <summary>
    /// Gets the point associated with the event.
    /// </summary>
    public Vector2 Point { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the point is the
    /// left (source) endpoint of the segment (p, other->p).
    /// </summary>
    public bool Left { get; set; }

    /// <summary>
    /// Gets index of the polygon to which the associated segment belongs to;
    /// </summary>
    public int Polygon { get; }

    /// <summary>
    /// Gets a unique id for each event.
    /// </summary>
    internal long Id { get; }

    /// <summary>
    /// Gets or sets the event associated to the other endpoint of the segment.
    /// </summary>
    public SweepEvent OtherEvent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the segment (p, other->p) represent an
    /// inside-outside transition in the polygon for a vertical ray from (p.x, -infinite)
    /// that crosses the segment.
    /// </summary>
    public bool InOut { get; set; }

    /// <summary>
    /// Gets or sets the sorted sweep events. Only used in "left" events.
    /// Position of the event (segment) in SL (status line).
    /// </summary>
    public int PosSL { get; set; }

    /// <summary>
    /// Returns the segment associated with the sweep event.
    /// </summary>
    /// <returns>The <see cref="Segment"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Segment Segment() => new(this.Point, this.OtherEvent.Point);

    /// <summary>
    /// Is the line segment (point, otherEvent->point) below point p.
    /// </summary>
    /// <param name="p">The point to check against.</param>
    /// <returns>
    /// <see langword="true"/> if the line segment is below the point; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Below(Vector2 p)
        => this.Left
        ? PolygonUtilities.SignedArea(this.Point, this.OtherEvent.Point, p) > 0F
        : PolygonUtilities.SignedArea(this.OtherEvent.Point, this.Point, p) > 0F;

    /// <summary>
    /// Is the line segment (point, otherEvent->point) above point p.
    /// </summary>
    /// <param name="p">The point to check against.</param>
    /// <returns>
    /// <see langword="true"/> if the line segment is above the point; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Above(Vector2 p) => !this.Below(p);
}
