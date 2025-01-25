// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Text;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using PolygonClipper.Tests.TestCases;
using Xunit;

using GeoPolygon = GeoJSON.Text.Geometry.Polygon;

namespace PolygonClipper.Tests;
public class GenericTestCases
{
    public static IEnumerable<object[]> GetTestCases()
        => TestData.Generic.GetFileNames().Select(x => new object[] { x });

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void GenericTestCase(string testCaseFile)
    {
        // Arrange
        FeatureCollection data = TestData.Generic.GetFeatureCollection(testCaseFile);

        Assert.True(data.Features.Count >= 2, "Test case file must contain at least two features.");

        IGeometryObject subjectGeometry = data.Features[0].Geometry;
        IGeometryObject clippingGeometry = data.Features[1].Geometry;

        Polygon subject = ConvertToPolygon(subjectGeometry);
        Polygon clipping = ConvertToPolygon(clippingGeometry);

#pragma warning disable RCS1124 // Inline local variable
        List<ExpectedResult> expectedResults = ExtractExpectedResults(data.Features.Skip(2).ToList(), data.Type);
#pragma warning restore RCS1124 // Inline local variable

        foreach (ExpectedResult result in expectedResults)
        {
            Polygon expected = result.Coordinates;
            Polygon actual = result.Operation(subject, clipping);

            Assert.Equal(expected.ContourCount, actual.ContourCount);
            for (int i = 0; i < expected.ContourCount; i++)
            {
                // We don't test for holes here as the reference tests do not do so.
                Assert.Equal(expected[i].VertexCount, actual[i].VertexCount);
                for (int j = 0; j < expected[i].VertexCount; j++)
                {
                    Vertex expectedVertex = expected[i].GetVertex(j);
                    Vertex actualVertex = actual[i].GetVertex(j);
                    Assert.Equal(expectedVertex.X, actualVertex.X, 3);
                    Assert.Equal(expectedVertex.Y, actualVertex.Y, 3);
                }
            }
        }
    }

    private static Polygon ConvertToPolygon(IGeometryObject geometry)
    {
        if (geometry is GeoPolygon geoJsonPolygon)
        {
            // Convert GeoJSON Polygon to our Polygon type
            Polygon polygon = new();
            foreach (LineString ring in geoJsonPolygon.Coordinates)
            {
                Contour contour = new();
                foreach (IPosition xy in ring.Coordinates)
                {
                    contour.AddVertex(new Vertex(xy.Longitude, xy.Latitude));
                }
                polygon.Push(contour);
            }

            return polygon;
        }
        else if (geometry is MultiPolygon geoJsonMultiPolygon)
        {
            // Convert GeoJSON MultiPolygon to our Polygon type
            Polygon polygon = new();
            foreach (GeoPolygon geoPolygon in geoJsonMultiPolygon.Coordinates)
            {
                foreach (LineString ring in geoPolygon.Coordinates)
                {
                    Contour contour = new();
                    foreach (IPosition xy in ring.Coordinates)
                    {
                        contour.AddVertex(new Vertex(xy.Longitude, xy.Latitude));
                    }
                    polygon.Push(contour);
                }
            }

            return polygon;
        }

        throw new InvalidOperationException("Unsupported geometry type.");
    }

    private static List<ExpectedResult> ExtractExpectedResults(List<Feature> features, GeoJSONObjectType type)
        => features.ConvertAll(feature =>
        {
            string mode = feature.Properties["operation"]?.ToString();
            Func<Polygon, Polygon, Polygon> operation = mode switch
            {
                "union" => PolygonClipper.Union,
                "intersection" => PolygonClipper.Intersection,
                "xor" => PolygonClipper.Xor,
                "diff" => PolygonClipper.Difference,
                "diff_ba" => (a, b) => PolygonClipper.Difference(b, a),
                _ => throw new InvalidOperationException($"Invalid mode: {mode}")
            };

            if (type == GeoJSONObjectType.Polygon)
            {
                return new ExpectedResult
                {
                    Operation = operation,
                    Coordinates = ConvertToPolygon(feature.Geometry as GeoPolygon)
                };
            }

            return new ExpectedResult
            {
                Operation = operation,
                Coordinates = ConvertToPolygon(feature.Geometry as MultiPolygon)
            };
        });

    private class ExpectedResult
    {
        public Func<Polygon, Polygon, Polygon> Operation { get; set; }
        public Polygon Coordinates { get; set; }
    }

    private enum TestType
    {
        Polygon = 0,

        MultiPolygon = 1
    }
}
