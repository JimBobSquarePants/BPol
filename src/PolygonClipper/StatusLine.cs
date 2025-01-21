// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PolygonClipper;

/// <summary>
/// Represents a status line for the sweep line algorithm, maintaining a sorted collection of sweep events.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
internal sealed class StatusLine
{
    private readonly List<SweepEvent> sortedEvents = new();
    private readonly SegmentComparer comparer = new();

    /// <summary>
    /// Gets the number of events in the status line.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.sortedEvents.Count;
    }

    /// <summary>
    /// Gets the event at the specified index.
    /// </summary>
    /// <param name="index">The index of the event.</param>
    /// <returns>The sweep event at the given index.</returns>
    public SweepEvent this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.sortedEvents[index];
    }

    /// <summary>
    /// Inserts a sweep event into the status line, maintaining sorted order.
    /// </summary>
    /// <param name="e">The sweep event to insert.</param>
    /// <returns>The index where the event was inserted.</returns>
    public int Insert(SweepEvent e)
    {
        int index = this.sortedEvents.BinarySearch(e, this.comparer);
        if (index < 0)
        {
            index = ~index; // Get the correct insertion point
        }

        this.sortedEvents.Insert(index, e);
        this.UpdateIndices();
        return index;
    }

    /// <summary>
    /// Removes a sweep event from the status line.
    /// </summary>
    /// <param name="index">The index of the event to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the number of events.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        this.sortedEvents.RemoveAt(index);
        this.UpdateIndices();
    }

    /// <summary>
    /// Updates the <see cref="SweepEvent.PosSL"/> property for all events in the list.
    /// </summary>
    private void UpdateIndices()
    {
        List<SweepEvent> e = this.sortedEvents;
        for (int i = 0; i < this.sortedEvents.Count; i++)
        {
            e[i].PosSL = i;

            // Ensure paired events also reference the correct position
            if (e[i].OtherEvent != null)
            {
                e[i].OtherEvent.PosSL = i;
            }
        }
    }
}
