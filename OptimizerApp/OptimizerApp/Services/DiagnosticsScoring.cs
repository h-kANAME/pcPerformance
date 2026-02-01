using OptimizerApp.Models;

namespace OptimizerApp.Services;

public static class DiagnosticsScoring
{
    public static (int score, HealthStatus status) Calculate(
        double? cpuUsage,
        double? ramUsage,
        double? diskFreePercent)
    {
        if (cpuUsage is null && ramUsage is null && diskFreePercent is null)
        {
            return (0, HealthStatus.Unknown);
        }

        var score = 100;
        score -= cpuUsage is >= 90 ? 30 : cpuUsage is >= 80 ? 20 : cpuUsage is >= 70 ? 10 : 0;
        score -= ramUsage is >= 95 ? 30 : ramUsage is >= 85 ? 20 : ramUsage is >= 75 ? 10 : 0;
        score -= diskFreePercent is <= 10 ? 30 : diskFreePercent is <= 15 ? 20 : diskFreePercent is <= 20 ? 10 : 0;

        score = Math.Clamp(score, 0, 100);
        var status = score >= 70 ? HealthStatus.Ok : score >= 40 ? HealthStatus.Warning : HealthStatus.Critical;
        return (score, status);
    }
}
