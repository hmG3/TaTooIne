using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

namespace TaTooIne.Benchmark;

internal sealed class BenchmarkParamsColumnProvider : IColumnProvider
{
    private BenchmarkParamsColumnProvider()
    {
    }

    public static BenchmarkParamsColumnProvider Instance { get; } = new();

    public IEnumerable<IColumn> GetColumns(Summary summary) =>
        summary.BenchmarksCases
            .SelectMany(c => c.Parameters.Items.Select(item => item.Definition))
            .Where(c => c.Name != "order")
            .Select(definition => new ParamColumn(definition.Name, definition.PriorityInCategory));
}