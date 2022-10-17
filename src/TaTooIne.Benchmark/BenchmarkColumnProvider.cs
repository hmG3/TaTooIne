using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

namespace TaTooIne.Benchmark;

internal sealed class BenchmarkColumnProvider : IColumnProvider
{
    public IEnumerable<IColumn> GetColumns(Summary summary) => summary.BenchmarksCases.SelectMany(b =>
        b.Parameters.Items.Select(item => { return item.Definition; })).Distinct().Select(definition =>
    {
        return new ParamColumn(definition.Name, definition.PriorityInCategory);
    });
}
