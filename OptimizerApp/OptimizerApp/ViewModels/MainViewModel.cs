namespace OptimizerApp.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    public DiagnosticsViewModel Diagnostics { get; }
    public PrePlayOptimizationViewModel PrePlayOptimization { get; }
    public MemoryCleanupViewModel MemoryCleanup { get; }
    public DiskOptimizationViewModel DiskOptimization { get; }

    public MainViewModel(DiagnosticsViewModel diagnostics, PrePlayOptimizationViewModel prePlayOptimization, MemoryCleanupViewModel memoryCleanup, DiskOptimizationViewModel diskOptimization)
    {
        Diagnostics = diagnostics;
        PrePlayOptimization = prePlayOptimization;
        MemoryCleanup = memoryCleanup;
        DiskOptimization = diskOptimization;
    }
}
