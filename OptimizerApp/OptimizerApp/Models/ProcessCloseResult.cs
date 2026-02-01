namespace OptimizerApp.Models;

public sealed class ProcessCloseResult
{
    public required string Name { get; init; }
    public int ClosedCount { get; init; }
    public int FailedCount { get; init; }
}
