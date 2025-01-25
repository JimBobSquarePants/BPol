// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PolygonClipper;

/// <summary>
/// Represents a stable priority queue that maintains the order of items with the same priority.
/// </summary>
/// <typeparam name="T">The type of elements in the priority queue.</typeparam>
/// <typeparam name="TComparer">The type of comparer used to determine the priority of the elements.</typeparam>
[DebuggerDisplay("Count = {Count}")]
internal sealed class StablePriorityQueue<T, TComparer>
    where TComparer : IComparer<T>
{
    private readonly List<T> heap = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StablePriorityQueue{T, TComparer}"/> class with a specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to determine the priority of the elements.</param>
    public StablePriorityQueue(TComparer comparer)
        => this.Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    /// <summary>
    /// Gets the number of elements in the priority queue.
    /// </summary>
    public int Count => this.heap.Count;

    /// <summary>
    /// Gets the comparer used to determine the priority of the elements.
    /// </summary>
    public TComparer Comparer { get; }

    /// <summary>
    /// Adds an item to the priority queue, maintaining the heap property.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Enqueue(T item)
    {
        this.heap.Add(item);
        this.Up(this.heap.Count - 1);
    }

    /// <summary>
    /// Removes and returns the item with the highest priority (lowest value) from the priority queue.
    /// </summary>
    /// <returns>The item with the highest priority.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the priority queue is empty.</exception>
    public T Dequeue()
    {
        if (this.heap.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        T top = this.heap[0];
        T bottom = this.heap[^1];
        this.heap.RemoveAt(this.heap.Count - 1);

        if (this.heap.Count > 0)
        {
            this.heap[0] = bottom;
            this.Down(0);
        }

        return top;
    }

    /// <summary>
    /// Returns the item with the highest priority (lowest value) without removing it.
    /// </summary>
    /// <returns>The item with the highest priority.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the priority queue is empty.</exception>
    public T Peek()
    {
        if (this.heap.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return this.heap[0];
    }

    /// <summary>
    /// Restores the heap property by moving the item at the specified index upward.
    /// </summary>
    /// <param name="index">The index of the item to move upward.</param>
    private void Up(int index)
    {
        List<T> data = this.heap;
        T item = data[index];
        TComparer comparer = this.Comparer;

        while (index > 0)
        {
            int parent = (index - 1) >> 1;
            T current = data[parent];
            if (comparer.Compare(item, current) >= 0)
            {
                break;
            }

            data[index] = current;
            index = parent;
        }

        data[index] = item;
    }

    /// <summary>
    /// Restores the heap property by moving the item at the specified index downward.
    /// </summary>
    /// <param name="index">The index of the item to move downward.</param>
    private void Down(int index)
    {
        List<T> data = this.heap;
        int halfLength = data.Count >> 1;
        T item = data[index];
        TComparer comparer = this.Comparer;

        while (index < halfLength)
        {
            int bestChild = (index << 1) + 1; // Initially left child
            int right = bestChild + 1;

            if (right < data.Count && comparer.Compare(data[right], data[bestChild]) < 0)
            {
                bestChild = right;
            }

            if (comparer.Compare(data[bestChild], item) >= 0)
            {
                break;
            }

            data[index] = data[bestChild];
            index = bestChild;
        }

        data[index] = item;
    }
}
