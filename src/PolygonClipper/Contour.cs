// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Numerics;

namespace PolygonClipper;

/// <summary>
/// Represents a simple polygon. The edges of the contours are interior disjoint.
/// </summary>
public sealed class Contour
{
    private bool precomputeCC;
    private bool cc;

    /// <summary>
    /// Set of points conforming the external contour
    /// </summary>
    private readonly List<Vector2> points = new();

    /// <summary>
    /// Holes of the contour. They are stored as the indexes of
    /// the holes in a polygon class
    /// </summary>
    private readonly List<int> holes = new();

    /// <summary>
    /// Gets the number of vertices.
    /// </summary>
    public int NVertices => this.points.Count;

    /// <summary>
    /// Gets the number of edges.
    /// </summary>
    public int NEdges => this.points.Count;

    /// <summary>
    /// Gets the number of holes.
    /// </summary>
    public int NHoles => this.holes.Count;

    /// <summary>
    /// Gets or sets a value indicating whether the contour
    /// is external. i.e not a hole.
    /// </summary>
    public bool External { get; set; } = true;

    /// <summary>
    /// Get the p-th vertex of the external contour.
    /// </summary>
    /// <param name="p">The index of the vertex.</param>
    /// <returns>The <see cref="Vector2"/>.</returns>
    public Vector2 Vertex(int p) => this.points[p];

    /// <summary>
    /// Get the p-th hole of the contour.
    /// </summary>
    /// <param name="p">The index of the hole.</param>
    /// <returns>The <see cref="Vector2"/>.</returns>
    public int Hole(int p) => this.holes[p];

    /// <summary>
    /// Gets the p-th segment of the contour.
    /// </summary>
    /// <param name="p">The index of the segment.</param>
    /// <returns>The <see cref="Segment"/>.</returns>
    internal Segment Segment(int p)
        => (p == this.NVertices - 1)
        ? new Segment(this.points[^1], this.points[0])
        : new Segment(this.points[p], this.points[p + 1]);

    /// <summary>
    /// Gets the bounding box.
    /// </summary>
    /// <returns>The <see cref="Box2"/>.</returns>
    internal Box2 BBox()
    {
        if (this.NVertices == 0)
        {
            return default;
        }

        Box2 b = new(this.Vertex(0));
        for (int i = 1; i < this.NVertices; ++i)
        {
            b = b.Add(new Box2(this.Vertex(i)));
        }

        return b;
    }

    /// <summary>
    /// Gets a value indicating whether the contour is counterclockwise oriented
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the contour is counterclockwise oriented; otherwise <see langword="false"/>.
    /// </returns>
    public bool CounterClockwise()
    {
        if (this.precomputeCC)
        {
            return this.cc;
        }

        this.precomputeCC = true;

        float area = 0F;
        Vector2 c;
        Vector2 c1;
        for (int i = 0; i < this.NVertices - 1; i++)
        {
            c = this.Vertex(i);
            c1 = this.Vertex(i + 1);
            area += (c.X * c1.Y) - (c1.X * c.Y);
        }

        c = this.Vertex(this.NVertices - 1);
        c1 = this.Vertex(0);
        area += (c.X * c1.Y) - (c1.X * c.Y);
        return this.cc = area >= 0F;
    }

    /// <summary>
    /// Gets a value indicating whether the contour is clockwise oriented
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the contour is clockwise oriented; otherwise <see langword="false"/>.
    /// </returns>
    public bool Clockwise() => !this.CounterClockwise();

    /// <summary>
    /// Changes the contour orientation.
    /// </summary>
    public void ChangeOrientation()
    {
        this.points.Reverse();
        this.cc = !this.cc;
    }

    /// <summary>
    /// Sets the contour to clockwise orientation.
    /// </summary>
    public void SetClockwise()
    {
        if (this.CounterClockwise())
        {
            this.ChangeOrientation();
        }
    }

    /// <summary>
    /// Sets the contour to counterclockwise orientation.
    /// </summary>
    public void SetCounterClockwise()
    {
        if (this.Clockwise())
        {
            this.ChangeOrientation();
        }
    }

    /// <summary>
    /// Offsets the contour by the specified xy-coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Move(float x, float y)
    {
        for (int i = 0; i < this.points.Count; i++)
        {
            this.points[i] = Vector2.Add(this.points[i], new Vector2(x, y));
        }
    }

    /// <summary>
    /// Adds the point the end of the vertices collection.
    /// </summary>
    /// <param name="point">The point to add.</param>
    public void Add(Vector2 point) => this.points.Add(point);

    /// <summary>
    /// Removes the p-th vertex of the contour.
    /// </summary>
    /// <param name="p">The index of the vertex.</param>
    public void Erase(int p) => this.points.RemoveAt(p);

    /// <summary>
    /// Clears the vertices and holes.
    /// </summary>
    public void Clear()
    {
        this.points.Clear();
        this.holes.Clear();
    }

    /// <summary>
    /// Clears the holes.
    /// </summary>
    public void ClearHoles() => this.holes.Clear();

    /// <summary>
    /// Returns the last point in the contour.
    /// </summary>
    /// <returns>The <see cref="Vector2"/>.</returns>
    public Vector2 Back() => this.points[^1];

    /// <summary>
    /// Adds the point the end of the vertices collection.
    /// </summary>
    /// <param name="ind">The index of the hole to add.</param>
    public void AddHole(int ind) => this.holes.Add(ind);
}
