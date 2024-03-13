using Microsoft.AspNetCore.Components;
using TaTooIne.Demo.Models;

namespace TaTooIne.Demo.DemoIndicators;

public partial class IndicatorData
{
    [Parameter]
    public CalculatedState? State { get; set; }

    private IEnumerable<IEnumerable<Line?>> _transposedState = null!;

    protected override void OnParametersSet()
    {
        if (State != null)
        {
            _transposedState = Transpose(State.InputValues.Concat(State.OutputValues));
        }
    }

    private static IEnumerable<IEnumerable<T?>> Transpose<T>(IEnumerable<IEnumerable<T?>> source)
    {
        // ReSharper disable once NotDisposedResourceIsReturned
        var enumerators = source.Select(e => e.GetEnumerator()).Where(e => e.MoveNext()).ToList();

        try
        {
            while (enumerators.Count != 0)
            {
                yield return enumerators.Select(e => e.Current);
                enumerators = enumerators.Where(e => e.MoveNext()).ToList();
            }
        }
        finally
        {
            enumerators.ForEach(e => e.Dispose());
        }
    }
}
