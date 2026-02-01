using OptimizerApp.Models;

namespace OptimizerApp.Services;

public interface ISystemDiagnosticsService
{
    DiagnosticsSnapshot GetSnapshot();
    IReadOnlyList<ProcessUsageInfo> GetTopProcesses(int count);
    IReadOnlyList<StartupAppInfo> GetStartupApps();
    MemoryStats GetMemoryStats(int topProcessCount = 10);
    bool CleanMemory(IReadOnlyList<int> processIds);
    TempFileCleanupResult CleanTempFiles();
}
