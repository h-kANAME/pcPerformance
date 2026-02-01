using OptimizerApp.Models;
using OptimizerApp.Services;

namespace OptimizerApp.Tests;

public class DiagnosticsScoringTests
{
    [Fact]
    public void Calculate_WhenAllNull_ReturnsUnknown()
    {
        var (score, status) = DiagnosticsScoring.Calculate(null, null, null);

        Assert.Equal(0, score);
        Assert.Equal(HealthStatus.Unknown, status);
    }

    [Fact]
    public void Calculate_WhenHighCpuRamLowDisk_ReturnsCritical()
    {
        var (score, status) = DiagnosticsScoring.Calculate(95, 92, 8);

        Assert.InRange(score, 0, 40);
        Assert.Equal(HealthStatus.Critical, status);
    }

    [Fact]
    public void Calculate_WhenHealthy_ReturnsOk()
    {
        var (score, status) = DiagnosticsScoring.Calculate(15, 30, 45);

        Assert.InRange(score, 70, 100);
        Assert.Equal(HealthStatus.Ok, status);
    }
}
