namespace TaTooIne.Demo.Models;

public sealed record Line
{
    public required DateTime Time { get; init; }

    public required double Value { get; init; }
}
