using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using TaTooIne.Benchmark;

var config = ManualConfig.CreateEmpty()
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByParams)
    .WithOrderer(BenchmarkOrderer.Instance)
    .WithSummaryStyle(SummaryStyle.Default.WithTimeUnit(TimeUnit.Millisecond))
    .WithUnionRule(ConfigUnionRule.Union);
#if DEBUG
config = ManualConfig.Union(new DebugInProcessConfig(), config);
#else
config = ManualConfig.Union(config, ManualConfig.CreateEmpty()
    .AddLogger(ConsoleLogger.Default)
    .AddJob(Job.Default.WithRuntime(CoreRuntime.Core80))
    .AddColumnProvider([
        DefaultColumnProviders.Descriptor,
        DefaultColumnProviders.Job,
        DefaultColumnProviders.Statistics,
        BenchmarkParamsColumnProvider.Instance,
        DefaultColumnProviders.Metrics
    ])
    .AddExporter(HtmlExporter.Default));
#endif

BenchmarkRunner.Run<IndicatorsBenchmark>(config);
