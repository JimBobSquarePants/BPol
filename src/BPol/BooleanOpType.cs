// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace BPol;

/// <summary>
/// Specifies the type of boolean operation to perform on polygons.
/// </summary>
public enum BooleanOpType
{
    /// <summary>
    /// The intersection operation, which results in the area common to both polygons.
    /// </summary>
    INTERSECTION = 0,

    /// <summary>
    /// The union operation, which results in the combined area of both polygons.
    /// </summary>
    UNION = 1,

    /// <summary>
    /// The difference operation, which subtracts the clipping polygon from the subject polygon.
    /// </summary>
    DIFFERENCE = 2,

    /// <summary>
    /// The exclusive OR (XOR) operation, which results in the area covered by exactly one polygon,
    /// excluding the overlapping areas.
    /// </summary>
    XOR = 3
}
