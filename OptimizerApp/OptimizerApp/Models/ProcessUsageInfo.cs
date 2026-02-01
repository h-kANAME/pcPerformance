namespace OptimizerApp.Models;

public sealed class ProcessUsageInfo
{
    public int ProcessId { get; init; }
    public required string Name { get; init; }
    public double? CpuPercent { get; init; }
    public double MemoryMb { get; init; }
}
