namespace OptimizerApp.Models;

public sealed class PrePlayResult
{
    public DiagnosticsSnapshot Before { get; init; } = new();
    public DiagnosticsSnapshot After { get; init; } = new();
    public IReadOnlyList<ProcessCloseResult> Closed { get; init; } = Array.Empty<ProcessCloseResult>();
    public IReadOnlyList<string> Skipped { get; init; } = Array.Empty<string>();
}
