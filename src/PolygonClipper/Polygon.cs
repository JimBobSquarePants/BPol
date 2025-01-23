// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PolygonClipper;

/// <summary>
/// Represents a complex polygon.
/// </summary>
public sealed class Polygon
{
    /// <summary>
    /// The collection of contours that make up the polygon.
    /// </summary>
    private readonly List<Contour> contours = new();

    /// <summary>
    /// Gets the number of contours in the polygon.
    /// </summary>
    public int ContourCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.contours.Count;
    }

    /// <summary>
    /// Gets the contour at the specified index.
    /// </summary>
    /// <param name="index">The index of the contour.</param>
    /// <returns>The <see cref="GetContour"/> at the given index.</returns>
    public Contour this[int index] => this.contours[index];

    /// <summary>
    /// Gets the total number of vertices across all contours in the polygon.
    /// </summary>
    /// <returns>The total vertex count.</returns>
    public int GetVertexCount()
    {
        int nv = 0;
        for (int i = 0; i < this.ContourCount; i++)
        {
            nv += this.contours[i].VertexCount;
        }

        return nv;
    }

    /// <summary>
    /// Joins another polygon to this instance.
    /// </summary>
    /// <param name="polygon">The polygon to join.</param>
    public void Join(Polygon polygon)
    {
        int size = this.ContourCount;
        for (int i = 0; i < polygon.contours.Count; ++i)
        {
            Contour contour = polygon.contours[i];
            this.Push(contour);
            this.Last().ClearHoles();

            for (int j = 0; j < contour.HoleCount; ++j)
            {
                this.Last().AddHoleIndex(contour.GetHoleIndex(j) + size);
            }
        }
    }

    /// <summary>
    /// Gets the bounding box.
    /// </summary>
    /// <returns>The <see cref="Box2"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2 GetBoundingBox()
    {
        if (this.ContourCount == 0)
        {
            return default;
        }

        Box2 b = this.contours[0].GetBoundingBox();
        for (int i = 1; i < this.ContourCount; i++)
        {
            b = b.Add(this.contours[i].GetBoundingBox());
        }

        return b;
    }

    /// <summary>
    /// Offsets the polygon by the specified x and y values.
    /// </summary>
    /// <param name="x">The x-coordinate offset.</param>
    /// <param name="y">The y-coordinate offset.</param>
    public void Offset(float x, float y)
    {
        for (int i = 0; i < this.contours.Count; i++)
        {
            this.contours[i].Offset(x, y);
        }
    }

    /// <summary>
    /// Adds a contour to the end of the contour collection.
    /// </summary>
    /// <param name="contour">The contour to add.</param>
    public void Push(Contour contour) => this.contours.Add(contour);

    /// <summary>
    /// Gets the last contour in the polygon.
    /// </summary>
    /// <returns>The last <see cref="Contour"/> in the collection.</returns>
    public Contour Last() => this.contours[^1];

    /// <summary>
    /// Removes the last contour from the polygon.
    /// </summary>
    public void Pop() => this.contours.RemoveAt(this.contours.Count - 1);

    /// <summary>
    /// Clears all contours from the polygon.
    /// </summary>
    public void Clear() => this.contours.Clear();

    /// <summary>
    /// Computes the holes within the polygon.
    /// </summary>
    public void ComputeHoles()
    {
        if (this.ContourCount < 2)
        {
            Contour root = this.contours[0];
            if (this.ContourCount == 1 && root.IsClockwise())
            {
                root.Reverse();
            }

            return;
        }

        int initCapacity = this.GetVertexCount() * 2;
        List<SweepEvent> ev = new(initCapacity);
        List<SweepEvent> evp = new(initCapacity);
        List<Contour> contours = this.contours;
        for (int i = 0; i < contours.Count; i++)
        {
            Contour contour = contours[i];
            contour.SetCounterClockwise();
            for (int j = 0; j < contour.EdgeCount; j++)
            {
                Segment s = contour.Segment(j);
                if (s.IsVertical())
                {
                    // Vertical segments are not processed.
                    continue;
                }

                ev.Add(new SweepEvent(s.Source, true, i));
                ev.Add(new SweepEvent(s.Target, true, i));
                SweepEvent se1 = ev[^2];
                SweepEvent se2 = ev[^1];
                se1.OtherEvent = se2;
                se2.OtherEvent = se1;

                if (se1.Point.X < se2.Point.X)
                {
                    se2.Left = false;
                    se1.InOut = false;
                }
                else
                {
                    se1.Left = false;
                    se1.InOut = true;
                }

                evp.Add(se1);
                evp.Add(se2);
            }
        }

        evp.Sort(new SweepEventComparer());

        StatusLine statusLine = new(); // Status line.
        Span<bool> processed = new bool[this.ContourCount];
        Span<int> holeOf = new int[this.ContourCount]; // -1;
        holeOf.Fill(-1);

        int nProcessed = 0;
        for (int i = 0; i < evp.Count && nProcessed < contours.Count; i++)
        {
            SweepEvent e = evp[i];

            if (e.Left)
            {
                // The segment must be inserted into S
                e.PosSL = statusLine.Insert(e);

                if (!processed[e.ContourId])
                {
                    processed[e.ContourId] = true;
                    nProcessed++;
                    int prev = e.PosSL;

                    if (prev == 0)
                    {
                        contours[e.ContourId].SetCounterClockwise();
                    }
                    else
                    {
                        // Get the preceding event
                        SweepEvent prevEvent = statusLine[--prev];
                        Contour contour = contours[e.ContourId];
                        Contour prevContour = contours[prevEvent.ContourId];
                        if (!prevEvent.InOut)
                        {
                            holeOf[e.ContourId] = prevEvent.ContourId;
                            contour.IsExternal = false;
                            prevContour.AddHoleIndex(e.ContourId);

                            if (prevContour.IsCounterClockwise())
                            {
                                contour.SetClockwise();
                            }
                            else
                            {
                                contour.SetCounterClockwise();
                            }
                        }
                        else if (holeOf[prevEvent.ContourId] != -1)
                        {
                            holeOf[e.ContourId] = holeOf[prevEvent.ContourId];
                            contour.IsExternal = false;
                            Contour hole = contours[holeOf[e.ContourId]];
                            hole.AddHoleIndex(e.ContourId);

                            if (hole.IsCounterClockwise())
                            {
                                contour.SetClockwise();
                            }
                            else
                            {
                                contour.SetCounterClockwise();
                            }
                        }
                        else
                        {
                            contour.SetCounterClockwise();
                        }
                    }
                }
            }
            else
            {
                // The segment must be removed from S
                statusLine.RemoveAt(e.OtherEvent.PosSL);
            }
        }
    }
}
