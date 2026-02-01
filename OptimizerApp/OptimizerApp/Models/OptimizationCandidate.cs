namespace OptimizerApp.Models;

public sealed class OptimizationCandidate
{
    public required string Name { get; init; }
    public required string Reason { get; init; }
    public required string Description { get; init; }
    public required string SavingsDescription { get; init; }
    public int ProcessCount { get; init; }
    public double MemoryMb { get; init; }
    public double? CpuPercent { get; init; }
    public bool IsCritical { get; init; }
    public bool IsRunning => ProcessCount > 0;
}
