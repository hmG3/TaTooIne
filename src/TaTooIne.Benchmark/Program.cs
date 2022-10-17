using BenchmarkDotNet.Running;

namespace TaTooIne.Benchmark;

internal static class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<IndicatorsBenchmark>(new BenchmarkConfig());
    }
}
