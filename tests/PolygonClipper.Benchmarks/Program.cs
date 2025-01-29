// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Running;

namespace PolygonClipper.Benchmarks;

internal class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher
        .FromAssembly(typeof(Program).Assembly)
        .Run(args);
}
