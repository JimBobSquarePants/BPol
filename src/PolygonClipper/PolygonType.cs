// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace PolygonClipper;

/// <summary>
/// Specifies the type of a polygon in a boolean operation.
/// </summary>
public enum PolygonType
{
    /// <summary>
    /// Represents the subject polygon in a boolean operation.
    /// </summary>
    SUBJECT = 0,

    /// <summary>
    /// Represents the clipping polygon in a boolean operation.
    /// </summary>
    CLIPPING = 1
}
