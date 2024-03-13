namespace TaTooIne.Demo.Models;

public sealed class CalculatedState
{
    public required string FuncDescription { get; init; }

    public required IReadOnlyCollection<string> InputNames { get; init; }

    public required IReadOnlyCollection<string> OutputNames { get; init; }

    public required IReadOnlyCollection<IReadOnlyCollection<Line?>> InputValues { get; init; }

    public required List<List<Line?>> OutputValues { get; init; }
}
