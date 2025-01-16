// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace PolygonClipper;

/// <summary>
/// Specifies the type of an edge in a boolean operation on polygons.
/// </summary>
public enum EdgeType
{
    /// <summary>
    /// A normal edge that contributes to the resulting polygon.
    /// </summary>
    NORMAL = 0,

    /// <summary>
    /// An edge that does not contribute to the resulting polygon.
    /// This typically occurs when the edge lies entirely inside another polygon.
    /// </summary>
    NON_CONTRIBUTING = 1,

    /// <summary>
    /// An edge that represents a transition within the same polygon,
    /// meaning it does not cross into another polygon.
    /// </summary>
    SAME_TRANSITION = 2,

    /// <summary>
    /// An edge that represents a transition between different polygons,
    /// meaning it crosses from one polygon to another.
    /// </summary>
    DIFFERENT_TRANSITION = 3
}
