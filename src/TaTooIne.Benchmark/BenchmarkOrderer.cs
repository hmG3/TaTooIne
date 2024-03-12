using System.Collections.Immutable;
using System.Text;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace TaTooIne.Benchmark;

internal sealed class BenchmarkOrderer : IOrderer
{
    private BenchmarkOrderer()
    {
    }

    public static BenchmarkOrderer Instance { get; } = new();

    public bool SeparateLogicalGroups => true;

    public IEnumerable<BenchmarkCase> GetExecutionOrder(
        ImmutableArray<BenchmarkCase> benchmarkCases,
        IEnumerable<BenchmarkLogicalGroupRule>? _)
    {
        return benchmarkCases.OrderBy(b => b.Parameters["order"]).ThenBy(b => b.Descriptor.WorkloadMethodDisplayInfo);
    }

    public IEnumerable<BenchmarkCase> GetSummaryOrder(
        ImmutableArray<BenchmarkCase> benchmarksCases,
        Summary summary)
    {
        var benchmarkLogicalGroups = benchmarksCases.GroupBy(b => GetLogicalGroupKey(benchmarksCases, b));
        foreach (var logicalGroup in GetLogicalGroupOrder(benchmarkLogicalGroups,
                     Enumerable.Empty<BenchmarkLogicalGroupRule>()))
        {
            foreach (var benchmark in logicalGroup.OrderBy(b => summary[b]!.ResultStatistics?.Mean ?? 0d))
            {
                yield return benchmark;
            }
        }
    }

    public string GetHighlightGroupKey(BenchmarkCase benchmarkCase) => GetGroupKey(benchmarkCase);

    public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> _, BenchmarkCase benchmarkCase) => GetGroupKey(benchmarkCase);

    public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(
        IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups,
        IEnumerable<BenchmarkLogicalGroupRule>? _)
    {
        return logicalGroups;
    }

    private static string GetGroupKey(BenchmarkCase benchmarkCase)
    {
        var inputSizeParam = benchmarkCase.Parameters.Items[0];
        var indicatorParam = benchmarkCase.Parameters.Items[1];

        var keyBuilder = new StringBuilder();
        keyBuilder.Append('[');
        keyBuilder.Append(inputSizeParam.Name).Append('=').Append(inputSizeParam.ToDisplayText());
        keyBuilder.Append(", ");

        var indicatorParamValue = indicatorParam.ToDisplayText();
        if (benchmarkCase.Descriptor.WorkloadMethodDisplayInfo == "TALib")
        {
            indicatorParamValue = indicatorParamValue switch
            {
                "Cmo" => "XXX", // TALib uses EMA, Tulip uses SMA
                "LinearReg" => "LinReg",
                "LinearRegIntercept" => "LinRegIntercept",
                "LinearRegSlope" => "LinRegSlope",
                "Mult" => "Mul",
                "PlusDI" => "Di",
                "PlusDM" => "Dm",
                "RocP" => "Roc",
                "Sar" => "Psar",
                "TRange" => "Tr",
                "WclPrice" => "WcPrice",
                _ => indicatorParamValue
            };
        }

        keyBuilder.Append(indicatorParam.Name).Append('=').Append(indicatorParamValue);
        keyBuilder.Append(']');

        return keyBuilder.ToString();
    }
}
