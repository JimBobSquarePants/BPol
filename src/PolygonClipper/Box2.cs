// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PolygonClipper;

/// <summary>
/// Represents a bounding box.
/// </summary>
internal readonly struct Box2 : IEquatable<Box2>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Box2"/> struct.
    /// </summary>
    /// <param name="vector">The xy-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2(Vector2 vector)
        : this(vector.X, vector.Y, vector.X, vector.Y)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Box2"/> struct.
    /// </summary>
    /// <param name="xMin">The minimum x-coordinate.</param>
    /// <param name="yMin">The minimum y-coordinate.</param>
    /// <param name="xMax">The maximum x-coordinate.</param>
    /// <param name="yMax">The maximum y-coordinate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2(float xMin, float yMin, float xMax, float yMax)
    {
        this.XMin = xMin;
        this.YMin = yMin;
        this.XMax = xMax;
        this.YMax = yMax;
    }

    /// <summary>
    /// Gets the minimum x-coordinate.
    /// </summary>
    public float XMin { get; }

    /// <summary>
    /// Gets the minimum y-coordinate.
    /// </summary>
    public float YMin { get; }

    /// <summary>
    /// Gets the maximum x-coordinate.
    /// </summary>
    public float XMax { get; }

    /// <summary>
    /// Gets the maximum y-coordinate.
    /// </summary>
    public float YMax { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Box2 left, Box2 right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Box2 left, Box2 right)
        => !(left == right);

    /// <summary>
    /// Adds another bounding box to this instance.
    /// </summary>
    /// <param name="other">The other box.</param>
    /// <returns>The summed <see cref="Box2"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box2 Add(Box2 other)
    {
        // TODO: Consider using Vector2 as MinMax properties.
        return new Box2(
            MathF.Min(this.XMin, other.XMin),
            MathF.Min(this.YMin, other.YMin),
            MathF.Min(this.XMax, other.XMax),
            MathF.Min(this.YMax, other.YMax));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
        => obj is Box2 box
        && this.Equals(box);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Box2 other)
        => this.XMin == other.XMin
        && this.XMax == other.XMax
        && this.YMin == other.YMin
        && this.YMax == other.YMax;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(this.XMin, this.XMax, this.YMin, this.YMax);
}
