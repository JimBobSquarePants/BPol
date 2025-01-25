// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace PolygonClipper;

/// <summary>
/// Represents a two-dimensional vertex with X and Y coordinates.
/// </summary>
public readonly struct Vertex : IEquatable<Vertex>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex"/> struct.
    /// </summary>
    /// <param name="x">The X-coordinate of the vertex.</param>
    /// <param name="y">The Y-coordinate of the vertex.</param>
    public Vertex(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Gets the X-coordinate of the vertex.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the Y-coordinate of the vertex.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    /// <remarks>The <see cref="op_Addition" /> method defines the addition operation for <see cref="Vector2" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator +(Vertex left, Vertex right)
        => AsVertexUnsafe(AsVector128Unsafe(left) + AsVector128Unsafe(right));

    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    /// <remarks>The <see cref="op_Subtraction" /> method defines the subtraction operation for <see cref="Vector2" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator -(Vertex left, Vertex right)
        => AsVertexUnsafe(AsVector128Unsafe(left) - AsVector128Unsafe(right));

    /// <summary>
    /// Returns a new vector whose values are the product of each pair of elements in two specified vertices.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The second vertex.</param>
    /// <returns>The element-wise product vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator *(Vertex left, Vertex right) => AsVertexUnsafe(AsVector128Unsafe(left) * AsVector128Unsafe(right));

    /// <summary>
    /// Multiplies the specified vertex by the specified scalar value.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator *(Vertex left, double right) => AsVertexUnsafe(AsVector128Unsafe(left) * right);

    /// <summary>
    /// Multiplies the specified vertex by the specified scalar value.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator *(double left, Vertex right) => right * left;

    /// <summary>
    /// Divides the first vertex by the second.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The second vertex.</param>
    /// <returns>The vertex that results from dividing <paramref name="left" /> by <paramref name="right" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator /(Vertex left, Vertex right) => AsVertexUnsafe(AsVector128Unsafe(left) / AsVector128Unsafe(right));

    /// <summary>
    /// Divides the specified vertex by a specified scalar value.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator /(Vertex left, double right) => AsVertexUnsafe(AsVector128Unsafe(left) / right);

    /// <summary>
    /// Divides the specified vertex by the specified scalar value.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex operator /(double left, Vertex right) => right / left;

    /// <summary>
    /// Determines whether two vertices are equal.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The second vertex.</param>
    /// <returns><see langword="true"/> if the vertices are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Vertex left, Vertex right) => left.Equals(right);

    /// <summary>
    /// Determines whether two vertices are not equal.
    /// </summary>
    /// <param name="left">The first vertex.</param>
    /// <param name="right">The second vertex.</param>
    /// <returns><see langword="true"/> if the vertices are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Vertex left, Vertex right) => !left.Equals(right);

    /// <summary>
    /// Returns the dot product of two vertices.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <returns>The <see cref="double"/> dot product.</returns>
    public static double Dot(Vertex a, Vertex b)
    {
        Vector128<double> a128 = AsVector128Unsafe(a);
        Vector128<double> b128 = AsVector128Unsafe(b);
        return Vector128.Dot(a128, b128);
    }

    /// <summary>
    /// Returns the cross product of two vertices.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <returns>The <see cref="double"/> cross product.</returns>
    public static double Cross(Vertex a, Vertex b)
        => (a.X * b.Y) - (a.Y * b.X);

    /// <summary>Computes the Euclidean distance between the two given vertices.</summary>
    /// <param name="value1">The first vertex.</param>
    /// <param name="value2">The second vertex.</param>
    /// <returns>The distance.</returns>
    public static double Distance(Vertex value1, Vertex value2)
        => double.Sqrt(DistanceSquared(value1, value2));

    /// <summary>Returns the Euclidean distance squared between two specified vertices.</summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <returns>The distance squared.</returns>
    public static double DistanceSquared(Vertex a, Vertex b)
        => (a - b).LengthSquared();

    /// <summary>
    /// Returns the length of the vertex.
    /// </summary>
    /// <returns>The vertex's length.</returns>
    /// <altmember cref="LengthSquared" />
    public double Length()
        => double.Sqrt(this.LengthSquared());

    /// <summary>Returns the length of the vertex squared.</summary>
    /// <returns>The vertex's length squared.</returns>
    /// <remarks>This operation offers better performance than a call to the <see cref="Length" /> method.</remarks>
    /// <altmember cref="Length" />
    public double LengthSquared()
        => Dot(this, this);

    /// <summary>
    /// Returns a vertex whose elements are the minimum of each of the pairs of elements in two specified vertices.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <returns>The minimized <see cref="Vertex"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex Min(Vertex a, Vertex b)
        => AsVertexUnsafe(Vector128.Min(AsVector128Unsafe(a), AsVector128Unsafe(b)));

    /// <summary>
    /// Returns a vertex whose elements are the maximum of each of the pairs of elements in two specified vertices.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <returns>The maximized <see cref="Vertex"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vertex Max(Vertex a, Vertex b)
        => AsVertexUnsafe(Vector128.Max(AsVector128Unsafe(a), AsVector128Unsafe(b)));

    /// <inheritdoc/>
    public bool Equals(Vertex other)
        => this.X == other.X && this.Y == other.Y;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Vertex vertex && this.Equals(vertex);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

    /// <inheritdoc/>
    public override string ToString() => $"Vertex [ X={this.X}, Y={this.Y} ]";

    private static Vector128<double> AsVector128Unsafe(Vertex value)
        => Unsafe.BitCast<Vertex, Vector128<double>>(value);

    private static Vertex AsVertexUnsafe(Vector128<double> value)
        => Unsafe.BitCast<Vector128<double>, Vertex>(value);
}
