namespace OptimizerApp.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    public DiagnosticsViewModel Diagnostics { get; }
    public PrePlayOptimizationViewModel PrePlayOptimization { get; }
    public MemoryCleanupViewModel MemoryCleanup { get; }

    public MainViewModel(DiagnosticsViewModel diagnostics, PrePlayOptimizationViewModel prePlayOptimization, MemoryCleanupViewModel memoryCleanup)
    {
        Diagnostics = diagnostics;
        PrePlayOptimization = prePlayOptimization;
        MemoryCleanup = memoryCleanup;
    }
}
