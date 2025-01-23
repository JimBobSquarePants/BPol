// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

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
    public int VertexCount => this.points.Count;

    /// <summary>
    /// Gets the number of edges.
    /// </summary>
    public int EdgeCount => this.points.Count;

    /// <summary>
    /// Gets the number of holes.
    /// </summary>
    public int HoleCount => this.holes.Count;

    /// <summary>
    /// Gets or sets a value indicating whether the contour
    /// is external (not a hole).
    /// </summary>
    public bool IsExternal { get; set; } = true;

    /// <summary>
    /// Gets the vertex at the specified index of the external contour.
    /// </summary>
    /// <param name="index">The index of the vertex.</param>
    /// <returns>The <see cref="Vector2"/>.</returns>
    public Vector2 GetVertex(int index) => this.points[index];

    /// <summary>
    /// Gets the hole index at the specified position in the contour.
    /// </summary>
    /// <param name="index">The index of the hole.</param>
    /// <returns>The hole index.</returns>
    public int GetHoleIndex(int index) => this.holes[index];

    /// <summary>
    /// Gets the segment at the specified index of the contour.
    /// </summary>
    /// <param name="index">The index of the segment.</param>
    /// <returns>The <see cref="Segment"/>.</returns>
    internal Segment Segment(int index)
        => (index == this.VertexCount - 1)
        ? new Segment(this.points[^1], this.points[0])
        : new Segment(this.points[index], this.points[index + 1]);

    /// <summary>
    /// Gets the bounding box of the contour.
    /// </summary>
    /// <returns>The <see cref="Box2"/>.</returns>
    public Box2 GetBoundingBox()
    {
        if (this.VertexCount == 0)
        {
            return default;
        }

        Box2 b = new(this.GetVertex(0));
        for (int i = 1; i < this.VertexCount; ++i)
        {
            b = b.Add(new Box2(this.GetVertex(i)));
        }

        return b;
    }

    /// <summary>
    /// Gets a value indicating whether the contour is counterclockwise oriented
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the contour is counterclockwise oriented; otherwise <see langword="false"/>.
    /// </returns>
    public bool IsCounterClockwise()
    {
        if (this.precomputeCC)
        {
            return this.cc;
        }

        this.precomputeCC = true;

        float area = 0F;
        Vector2 c;
        Vector2 c1;
        for (int i = 0; i < this.VertexCount - 1; i++)
        {
            c = this.GetVertex(i);
            c1 = this.GetVertex(i + 1);
            area += (c.X * c1.Y) - (c1.X * c.Y);
        }

        c = this.GetVertex(this.VertexCount - 1);
        c1 = this.GetVertex(0);
        area += (c.X * c1.Y) - (c1.X * c.Y);
        return this.cc = area >= 0F;
    }

    /// <summary>
    /// Gets a value indicating whether the contour is clockwise oriented
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the contour is clockwise oriented; otherwise <see langword="false"/>.
    /// </returns>
    public bool IsClockwise() => !this.IsCounterClockwise();

    /// <summary>
    /// Reverses the orientation of the contour.
    /// </summary>
    public void Reverse()
    {
        this.points.Reverse();
        this.cc = !this.cc;
    }

    /// <summary>
    /// Sets the contour to clockwise orientation.
    /// </summary>
    public void SetClockwise()
    {
        if (this.IsCounterClockwise())
        {
            this.Reverse();
        }
    }

    /// <summary>
    /// Sets the contour to counterclockwise orientation.
    /// </summary>
    public void SetCounterClockwise()
    {
        if (this.IsClockwise())
        {
            this.Reverse();
        }
    }

    /// <summary>
    /// Offsets the contour by the specified x and y values.
    /// </summary>
    /// <param name="x">The x-coordinate offset.</param>
    /// <param name="y">The y-coordinate offset.</param>
    public void Offset(float x, float y)
    {
        for (int i = 0; i < this.points.Count; i++)
        {
            this.points[i] = Vector2.Add(this.points[i], new Vector2(x, y));
        }
    }

    /// <summary>
    /// Adds a vertex to the end of the vertices collection.
    /// </summary>
    /// <param name="vertex">The vertex to add.</param>
    public void AddVertex(Vector2 vertex) => this.points.Add(vertex);

    /// <summary>
    /// Removes the vertex at the specified index from the contour.
    /// </summary>
    /// <param name="index">The index of the vertex to remove.</param>
    public void RemoveVertexAt(int index) => this.points.RemoveAt(index);

    /// <summary>
    /// Clears all vertices and holes from the contour.
    /// </summary>
    public void Clear()
    {
        this.points.Clear();
        this.holes.Clear();
    }

    /// <summary>
    /// Clears all holes from the contour.
    /// </summary>
    public void ClearHoles() => this.holes.Clear();

    /// <summary>
    /// Gets the last vertex in the contour.
    /// </summary>
    /// <returns>The last <see cref="Vector2"/> in the contour.</returns>
    public Vector2 GetLastVertex() => this.points[^1];

    /// <summary>
    /// Adds a hole index to the contour.
    /// </summary>
    /// <param name="index">The index of the hole to add.</param>
    public void AddHoleIndex(int index) => this.holes.Add(index);
}
