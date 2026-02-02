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
    private readonly DiskOptimizationViewModel _diskOptimizationViewModel;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            var diagnosticsService = new SystemDiagnosticsService();
            
            _diagnosticsViewModel = new DiagnosticsViewModel(diagnosticsService);
            _prePlayOptimizationViewModel = new PrePlayOptimizationViewModel(
                diagnosticsService,
                new PrePlayOptimizationService());
            _memoryCleanupViewModel = new MemoryCleanupViewModel(diagnosticsService);
            
            // Create a minimal disk view model without calling RefreshDrives
            try
            {
                var diskService = new DiskOptimizationService();
                _diskOptimizationViewModel = new DiskOptimizationViewModel(diskService);
            }
            catch (Exception diskEx)
            {
                System.Diagnostics.Debug.WriteLine($"Disk init error: {diskEx}");
                throw;
            }
            
            DataContext = new MainViewModel(
                _diagnosticsViewModel, 
                _prePlayOptimizationViewModel, 
                _memoryCleanupViewModel, 
                _diskOptimizationViewModel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow error: {ex}");
            throw;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _diagnosticsViewModel.Dispose();
    }
}