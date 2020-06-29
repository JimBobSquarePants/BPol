// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BPol
{
    /// <summary>
    /// Represents a complex polygon.
    /// </summary>
    public sealed class Polygon
    {
        // The set of contours conforming the polugon.
        private readonly List<Contour> contours = new List<Contour>();

        /// <summary>
        /// Gets the number of contours.
        /// </summary>
        public int NContours
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return this.contours.Count; }
        }

        /// <summary>
        /// Gets the number of contours.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NVertices()
        {
            int nv = 0;
            for (int i = 0; i < this.NContours; i++)
            {
                nv += this.contours[i].NVertices;
            }

            return nv;
        }

        /// <summary>
        /// Joins the other polygon to this instance.
        /// </summary>
        /// <param name="pol">The polygon to join.</param>
        public void Join(Polygon pol)
        {
            var size = this.NContours;
            for (int i = 0; i < pol.NContours; ++i)
            {
                this.PushBack(pol.Contour(i));
                this.Back().ClearHoles();

                for (int j = 0; j < pol.Contour(i).NHoles; ++j)
                {
                    this.Back().AddHole(pol.Contour(i).Hole(j) + size);
                }
            }
        }

        /// <summary>
        /// Get the p-th contour of the polygon.
        /// </summary>
        /// <param name="p">The index of the contour.</param>
        /// <returns>The <see cref="Contour"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Contour Contour(int p) => this.contours[p];

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        /// <returns>The <see cref="Box2"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box2 BBox()
        {
            if (this.NContours == 0)
            {
                return default;
            }

            Box2 b = this.contours[0].BBox();
            for (int i = 1; i < this.NContours; i++)
            {
                b = b.Add(this.contours[i].BBox());
            }

            return b;
        }

        /// <summary>
        /// Offsets the polygon by the specified xy-coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void Move(float x, float y)
        {
            for (int i = 0; i < this.contours.Count; i++)
            {
                this.contours[i].Move(x, y);
            }
        }

        /// <summary>
        /// Adds the contour the end of the collection.
        /// </summary>
        /// <param name="contour">The contour to add.</param>
        public void PushBack(Contour contour) => this.contours.Add(contour);

        /// <summary>
        /// Returns the last contour in the polygon.
        /// </summary>
        /// <returns>The <see cref="Contour"/>.</returns>
        public Contour Back() => this.contours[this.contours.Count - 1];

        /// <summary>
        /// Removes the contour at the end of the collection.
        /// </summary>
        public void PopBack() => this.contours.RemoveAt(this.contours.Count - 1);

        /// <summary>
        /// Clears the contours.
        /// </summary>
        public void Clear() => this.contours.Clear();

        /// <summary>
        /// Computes and holes in the polygon.
        /// </summary>
        public void ComputeHoles()
        {
            if (this.NContours < 2)
            {
                if (this.NContours == 1 && this.Contour(0).Clockwise())
                {
                    this.Contour(0).ChangeOrientation();
                }

                return;
            }

            int initCapacity = this.NVertices() * 2;
            List<SweepEvent> ev = new List<SweepEvent>(initCapacity);
            List<SweepEvent> evp = new List<SweepEvent>(initCapacity);
            for (int i = 0; i < this.NContours; i++)
            {
                Contour contour = this.Contour(i);
                contour.SetCounterClockwise();
                for (int j = 0; j < contour.NEdges; j++)
                {
                    Segment s = contour.Segment(j);
                    if (s.IsVertical())
                    {
                        // Vertical segments are not processed.
                        continue;
                    }

                    ev.Add(new SweepEvent(s.Source, true, i));
                    ev.Add(new SweepEvent(s.Target, true, i));
                    SweepEvent se1 = ev[ev.Count - 2];
                    SweepEvent se2 = ev[ev.Count - 1];
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

            evp.Sort(SegmentComparer.CompareEvents);

            SortedSet<SweepEvent> sl = new SortedSet<SweepEvent>(); // Status line.
            List<bool> processed = new List<bool>(this.NContours);
            List<int> holeOf = Repeated(-1, this.NContours); // -1;
            uint nProcessed = 0;

            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<T> Repeated<T>(T value, int count)
        {
            // Note we do not use an initial enumerable here to avoid copying.
            List<T> ret = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                ret.Add(value);
            }

            return ret;
        }
    }
}
