using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
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
    .WithSummaryStyle(new SummaryStyle(
        cultureInfo: CultureInfo.CurrentCulture,
        printUnitsInHeader: true,
        printUnitsInContent: false,
        timeUnit: TimeUnit.Millisecond,
        sizeUnit: SizeUnit.KB
    ))
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
    .AddExporter(HtmlExporter.Default, CsvExporter.Default, MarkdownExporter.Console));
#endif

BenchmarkRunner.Run<IndicatorsBenchmark>(config);
