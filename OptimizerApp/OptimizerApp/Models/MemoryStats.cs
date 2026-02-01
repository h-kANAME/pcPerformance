namespace OptimizerApp.Models;

public sealed class MemoryStats
{
    public double? CpuUsagePercent { get; init; }
    public double? RamUsedGb { get; init; }
    public double? RamTotalGb { get; init; }
    public double? RamUsagePercent { get; init; }
    public int ChromeProcessCount { get; init; }
    public double? ChromeRamGb { get; init; }
    public IReadOnlyList<ProcessUsageInfo> TopRamProcesses { get; init; } = [];
    public IReadOnlyList<ProcessUsageInfo> TopCpuProcesses { get; init; } = [];
    public IReadOnlyList<ProcessUsageInfo> ChromeProcesses { get; init; } = [];
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}
