namespace OptimizerApp.Models;

public sealed class StartupAppInfo
{
    public required string Name { get; init; }
    public required string Location { get; init; }
    public required string Command { get; init; }
}
