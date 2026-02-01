using System.Windows;
using OptimizerApp.Services;
using OptimizerApp.ViewModels;

namespace OptimizerApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DiagnosticsViewModel _diagnosticsViewModel;
    private readonly PrePlayOptimizationViewModel _prePlayOptimizationViewModel;
    private readonly MemoryCleanupViewModel _memoryCleanupViewModel;

    public MainWindow()
    {
        InitializeComponent();
        var diagnosticsService = new SystemDiagnosticsService();
        
        _diagnosticsViewModel = new DiagnosticsViewModel(diagnosticsService);
        _prePlayOptimizationViewModel = new PrePlayOptimizationViewModel(
            diagnosticsService,
            new PrePlayOptimizationService());
        _memoryCleanupViewModel = new MemoryCleanupViewModel(diagnosticsService);
        
        DataContext = new MainViewModel(_diagnosticsViewModel, _prePlayOptimizationViewModel, _memoryCleanupViewModel);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _diagnosticsViewModel.Dispose();
    }
}