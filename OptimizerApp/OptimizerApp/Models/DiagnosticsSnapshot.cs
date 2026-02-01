namespace OptimizerApp.Models;

public sealed class DiagnosticsSnapshot
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public double? CpuUsagePercent { get; init; }
    public double? RamUsagePercent { get; init; }
    public double? RamAvailableGb { get; init; }
    public double? DiskFreePercent { get; init; }
    public double? DiskFreeGb { get; init; }
    public int HealthScore { get; init; }
    public HealthStatus HealthStatus { get; init; } = HealthStatus.Unknown;
}
