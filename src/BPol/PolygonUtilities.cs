// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace BPol
{
    internal static class PolygonUtilities
    {
        /// <summary>
        /// Returns the signed area of a triangle.
        /// </summary>
        /// <param name="p0">The first point.</param>
        /// <param name="p1">The second point.</param>
        /// <param name="p2">The third point.</param>
        /// <returns>The <see cref="float"/> area.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedArea(Vector2 p0, Vector2 p1, Vector2 p2)
            => ((p0.X - p2.X) * (p1.Y - p2.Y)) - ((p1.X - p2.X) * (p0.Y - p2.Y));
    }
}
