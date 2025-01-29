// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

#nullable disable

using BenchmarkDotNet.Attributes;
using Clipper2Lib;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using PolygonClipper.Tests.TestCases;

using GeoPolygon = GeoJSON.Text.Geometry.Polygon;

namespace PolygonClipper.Benchmarks;

public class ClippingLibraryComparison
{
    private static readonly FeatureCollection Data = TestData.Generic.GetFeatureCollection("issue71.geojson");
    private Polygon subject;
    private Polygon clipping;

    private PathsD subject2;
    private PathsD clipping2;

    [GlobalSetup]
    public void Setup()
    {
        (Polygon subject, Polygon clipping) = BuildPolygon();
        this.subject = subject;
        this.clipping = clipping;

        (PathsD subject2, PathsD clipping2) = BuildPolygon2();
        this.subject2 = subject2;
        this.clipping2 = clipping2;
    }

    [Benchmark]
    public Polygon Clipper() => PolygonClipper.Union(this.subject, this.clipping);

    [Benchmark(Baseline = true)]
    public PathsD Clipper2()
    {
        PathsD solution = [];
        ClipperD clipper2 = new();
        clipper2.AddSubject(this.subject2);
        clipper2.AddClip(this.clipping2);
        bool x = clipper2.Execute(ClipType.Union, FillRule.EvenOdd, solution);
        if (!x)
        {
            throw new InvalidOperationException("Failed to clip polygons.");
        }
        return solution;
    }

    public static (Polygon Subject, Polygon Clipping) BuildPolygon()
    {
        IGeometryObject subjectGeometry = Data.Features[0].Geometry;
        IGeometryObject clippingGeometry = Data.Features[1].Geometry;

        Polygon subject = ConvertToPolygon(subjectGeometry);
        Polygon clipping = ConvertToPolygon(clippingGeometry);

        return (subject, clipping);
    }

    public static (PathsD Subject, PathsD Clipping) BuildPolygon2()
    {
        IGeometryObject subjectGeometry = Data.Features[0].Geometry;
        IGeometryObject clippingGeometry = Data.Features[1].Geometry;

        PathsD subject = ConvertToPolygon2(subjectGeometry);
        PathsD clipping = ConvertToPolygon2(clippingGeometry);

        return (subject, clipping);
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

    private static PathsD ConvertToPolygon2(IGeometryObject geometry)
    {
        if (geometry is GeoPolygon geoJsonPolygon)
        {
            // Convert GeoJSON Polygon to our Polygon type
            PathsD polygon = [];
            foreach (LineString ring in geoJsonPolygon.Coordinates)
            {
                PathD contour = [];
                foreach (IPosition xy in ring.Coordinates)
                {
                    contour.Add(new PointD(xy.Longitude, xy.Latitude));
                }
                polygon.Add(contour);
            }

            return polygon;
        }
        else if (geometry is MultiPolygon geoJsonMultiPolygon)
        {
            // Convert GeoJSON MultiPolygon to our Polygon type
            PathsD polygon = [];
            foreach (GeoPolygon geoPolygon in geoJsonMultiPolygon.Coordinates)
            {
                foreach (LineString ring in geoPolygon.Coordinates)
                {
                    PathD contour = [];
                    foreach (IPosition xy in ring.Coordinates)
                    {
                        contour.Add(new PointD(xy.Longitude, xy.Latitude));
                    }
                    polygon.Add(contour);
                }
            }

            return polygon;
        }

        throw new InvalidOperationException("Unsupported geometry type.");
    }
}
