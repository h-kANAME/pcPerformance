using OptimizerApp.Models;

namespace OptimizerApp.Services;

public interface IPrePlayOptimizationService
{
    IReadOnlyList<OptimizationCandidate> GetRunningProcesses();
    ProcessCloseResult CloseByName(string processName);
}
