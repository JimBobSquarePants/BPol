// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PolygonClipper;

/// <summary>
/// Represents a stable priority queue that maintains the order of items with the same priority.
/// </summary>
/// <typeparam name="T">The type of elements in the priority queue.</typeparam>
[DebuggerDisplay("Count = {Count}")]
internal sealed class StablePriorityQueue<T>
{
    private readonly List<T> heap = new();
    private readonly IComparer<T> comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="StablePriorityQueue{T}"/> class with a specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to determine the priority of the elements.</param>
    public StablePriorityQueue(IComparer<T> comparer)
        => this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    /// <summary>
    /// Gets the number of elements in the priority queue.
    /// </summary>
    public int Count => this.heap.Count;

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

        T result = this.heap[0];
        this.heap[0] = this.heap[^1];
        this.heap.RemoveAt(this.heap.Count - 1);
        this.Down(0);
        return result;
    }

    /// <summary>
    /// Restores the heap property by moving the item at the specified index upward.
    /// </summary>
    /// <param name="index">The index of the item to move upward.</param>
    private void Up(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) >> 1;
            if (this.comparer.Compare(this.heap[index], this.heap[parent]) >= 0)
            {
                break;
            }

            (this.heap[index], this.heap[parent]) = (this.heap[parent], this.heap[index]);
            index = parent;
        }
    }

    /// <summary>
    /// Restores the heap property by moving the item at the specified index downward.
    /// </summary>
    /// <param name="index">The index of the item to move downward.</param>
    private void Down(int index)
    {
        while (true)
        {
            int left = (index << 1) + 1;
            int right = left + 1;

            int smallest = index;

            if (left < this.heap.Count && this.comparer.Compare(this.heap[left], this.heap[smallest]) < 0)
            {
                smallest = left;
            }

            if (right < this.heap.Count && this.comparer.Compare(this.heap[right], this.heap[smallest]) < 0)
            {
                smallest = right;
            }

            if (smallest == index)
            {
                break;
            }

            (this.heap[index], this.heap[smallest]) = (this.heap[smallest], this.heap[index]);
            index = smallest;
        }
    }
}
