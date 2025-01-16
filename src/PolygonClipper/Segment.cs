// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PolygonClipper;

/// <summary>
/// Represents a line segment on a plane.
/// </summary>
public readonly struct Segment : IEquatable<Segment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct.
    /// </summary>
    /// <param name="source">The segment source.</param>
    /// <param name="target">The segment target.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Segment(Vector2 source, Vector2 target)
    {
        this.Source = source;
        this.Target = target;
        this.Min = Vector2.Min(source, target);
        this.Max = Vector2.Max(source, target);
    }

    /// <summary>
    /// Gets the segment source vector.
    /// </summary>
    public Vector2 Source { get; }

    /// <summary>
    /// Gets the segment target vector.
    /// </summary>
    public Vector2 Target { get; }

    /// <summary>
    /// Gets the point of the segment with lexicographically smallest coordinate.
    /// </summary>
    public Vector2 Min { get; }

    /// <summary>
    /// Gets the point of the segment with lexicographically largest coordinate.
    /// </summary>
    public Vector2 Max { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Segment left, Segment right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Segment left, Segment right)
        => !(left == right);

    /// <summary>
    /// Gets a value indicating whether the segment is degenerate.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the segment is degenerate; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Degenerate() => this.Source.Equals(this.Target);

    /// <summary>
    /// Gets a value indicating whether the segment is vertical.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the segment is vertical; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsVertical() => this.Source.X == this.Target.X;

    /// <summary>
    /// Changes the segment orientation.
    /// </summary>
    /// <returns>The <see cref="Segment"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Segment ChangeOrientation()
        => new(this.Target, this.Source);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
        => obj is Segment segment && this.Equals(segment);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Segment other)
        => this.Source.Equals(other.Source) && this.Target.Equals(other.Target);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(this.Source, this.Target);
}
