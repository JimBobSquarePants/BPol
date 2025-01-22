// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Diagnostics;
#nullable enable

namespace PolygonClipper;

/// <summary>
/// Represents a status line for the sweep line algorithm, maintaining a sorted collection of sweep events.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
internal sealed class StatusLine
{
    private readonly SortedList<SweepEvent, SweepEvent> sortedList = new(new SegmentComparer());

    /// <summary>
    /// Gets the number of events in the status line.
    /// </summary>
    public int Count => this.sortedList.Count;

    /// <summary>
    /// Adds a sweep event to the status line.
    /// </summary>
    /// <param name="e">The sweep event to add.</param>
    public void Add(SweepEvent e) => this.sortedList.Add(e, e);

    /// <summary>
    /// Removes a sweep event from the status line.
    /// </summary>
    /// <param name="e">The sweep event to remove.</param>
    public void Remove(SweepEvent e)
        => this.sortedList.Remove(e);

    /// <summary>
    /// Finds a sweep event in the status line.
    /// </summary>
    /// <param name="e">The sweep event to find.</param>
    /// <returns>The sweep event if found, otherwise <c>null</c>.</returns>
    public SweepEvent? Find(SweepEvent e)
        => this.sortedList.TryGetValue(e, out SweepEvent? result) ? result : null;

    /// <summary>
    /// Gets the previous event relative to the given event.
    /// </summary>
    /// <param name="e">The reference event.</param>
    /// <returns>The previous sweep event, or <c>null</c> if none exists.</returns>
    public SweepEvent? GetPrevious(SweepEvent e)
    {
        int index = this.sortedList.IndexOfKey(e);
        return index > 0 ? this.sortedList.Values[index - 1] : null;
    }

    /// <summary>
    /// Gets the next event relative to the given event.
    /// </summary>
    /// <param name="e">The reference event.</param>
    /// <returns>The next sweep event, or <c>null</c> if none exists.</returns>
    public SweepEvent? GetNext(SweepEvent e)
    {
        int index = this.sortedList.IndexOfKey(e);
        return index >= 0 && index < this.sortedList.Count - 1 ? this.sortedList.Values[index + 1] : null;
    }
}

//internal sealed class StatusLine
//{
//    private readonly List<SweepEvent> sortedEvents = new();
//    private readonly SegmentComparer comparer = new();

//    /// <summary>
//    /// Gets the number of events in the status line.
//    /// </summary>
//    public int Count
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        get => this.sortedEvents.Count;
//    }

//    /// <summary>
//    /// Gets the event at the specified index.
//    /// </summary>
//    /// <param name="index">The index of the event.</param>
//    /// <returns>The sweep event at the given index.</returns>
//    public SweepEvent this[int index]
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        get => this.sortedEvents[index];
//    }

//    /// <summary>
//    /// Inserts a sweep event into the status line, maintaining sorted order.
//    /// </summary>
//    /// <param name="e">The sweep event to insert.</param>
//    /// <returns>The index where the event was inserted.</returns>
//    public int Insert(SweepEvent e)
//    {
//        int index = this.sortedEvents.BinarySearch(e, this.comparer);
//        if (index < 0)
//        {
//            index = ~index; // Get the correct insertion point
//        }

//        this.sortedEvents.Insert(index, e);
//        this.Up(index);
//        return index;
//    }

//    /// <summary>
//    /// Removes a sweep event from the status line.
//    /// </summary>
//    /// <param name="index">The index of the event to remove.</param>
//    /// <exception cref="ArgumentOutOfRangeException">
//    /// Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the number of events.
//    /// </exception>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void RemoveAt(int index)
//    {
//        this.sortedEvents.RemoveAt(index);
//        this.Down(index);
//    }

//    private void Up(int index)
//    {
//        List<SweepEvent> e = this.sortedEvents;

//        for (int i = index + 1; i < e.Count; i++)
//        {
//            e[i].PosSL = i;
//        }
//    }

//    private void Down(int index)
//    {
//        List<SweepEvent> e = this.sortedEvents;

//        for (int i = index; i < e.Count; i++)
//        {
//            e[i].PosSL = i;
//        }
//    }
//}
