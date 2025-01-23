// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using Xunit;

namespace PolygonClipper.Tests;

public class PolygonUtilitiesTests
{
    [Fact]
    public void AnalyticalSignedArea()
    {
        // Assert negative area
        Assert.Equal(-1F, PolygonUtilities.SignedArea(new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)));

        // Assert positive area
        Assert.Equal(1F, PolygonUtilities.SignedArea(new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0)));

        // Assert collinear points, 0 area
        Assert.Equal(0F, PolygonUtilities.SignedArea(new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 2)));

        // Assert point on segment
        Assert.Equal(0F, PolygonUtilities.SignedArea(new Vector2(-1, 0), new Vector2(2, 3), new Vector2(0, 1)));

        // Assert point on segment (order reversed)
        Assert.Equal(0F, PolygonUtilities.SignedArea(new Vector2(2, 3), new Vector2(-1, 0), new Vector2(0, 1)));
    }
}
