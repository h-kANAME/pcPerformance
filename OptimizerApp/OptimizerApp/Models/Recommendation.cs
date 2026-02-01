namespace OptimizerApp.Models;

public sealed class Recommendation
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public HealthStatus Severity { get; init; } = HealthStatus.Warning;
}
