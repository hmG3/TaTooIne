using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace TaTooIne.Benchmark
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(new Job
            {
                Environment =
                {
                    Platform = Platform.X64,
                    Jit = Jit.RyuJit,
                    Runtime = CoreRuntime.Core31,
                }
            });
            AddColumnProvider(DefaultColumnProviders.Instance);
            AddColumn(RankColumn.Arabic);
            AddLogger(ConsoleLogger.Default);
            AddExporter(MarkdownExporter.Default);
            AddDiagnoser(MemoryDiagnoser.Default);
            Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
            UnionRule = ConfigUnionRule.AlwaysUseLocal;
        }
    }
}
