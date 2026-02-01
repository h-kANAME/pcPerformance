using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using OptimizerApp.Models;
using OptimizerApp.Services;

namespace OptimizerApp.ViewModels;

public sealed class MemoryCleanupViewModel : ObservableObject
{
    private readonly ISystemDiagnosticsService _diagnosticsService;
    private MemoryStats _currentStats = new();
    private bool _isLoading = false;
    private string _statusMessage = "Listo para monitorear";
    private bool _hasChrome = false;
    private bool _hasCleanupResults = false;
    private string _cleanupSummary = string.Empty;

    public MemoryStats CurrentStats
    {
        get => _currentStats;
        private set => SetProperty(ref _currentStats, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool HasChrome
    {
        get => _hasChrome;
        private set => SetProperty(ref _hasChrome, value);
    }

    public bool HasCleanupResults
    {
        get => _hasCleanupResults;
        private set => SetProperty(ref _hasCleanupResults, value);
    }

    public string CleanupSummary
    {
        get => _cleanupSummary;
        private set => SetProperty(ref _cleanupSummary, value);
    }

    public ObservableCollection<ProcessUsageInfo> TopRamProcesses { get; } = new();
    public ObservableCollection<ProcessUsageInfo> TopCpuProcesses { get; } = new();
    public ObservableCollection<ProcessUsageInfo> ChromeProcesses { get; } = new();

    public RelayCommand RefreshCommand { get; }
    public RelayCommand CleanTopRamCommand { get; }
    public RelayCommand CleanAllCommand { get; }
    public RelayCommand CleanTempFilesCommand { get; }

    public MemoryCleanupViewModel(ISystemDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;

        RefreshCommand = new RelayCommand(Refresh);
        CleanTopRamCommand = new RelayCommand(CleanTopRam, CanCleanTopRam);
        CleanAllCommand = new RelayCommand(CleanAll);
        CleanTempFilesCommand = new RelayCommand(CleanTempFiles);

        // Initial refresh
        Refresh();
    }

    private void Refresh()
    {
        IsLoading = true;
        StatusMessage = "Actualizando estadísticas...";

        try
        {
            CurrentStats = _diagnosticsService.GetMemoryStats(topProcessCount: 10);
            HasChrome = CurrentStats.ChromeProcessCount > 0;

            // Update observable collections
            TopRamProcesses.Clear();
            foreach (var process in CurrentStats.TopRamProcesses)
            {
                TopRamProcesses.Add(process);
            }

            TopCpuProcesses.Clear();
            foreach (var process in CurrentStats.TopCpuProcesses)
            {
                TopCpuProcesses.Add(process);
            }

            ChromeProcesses.Clear();
            foreach (var process in CurrentStats.ChromeProcesses)
            {
                ChromeProcesses.Add(process);
            }

            StatusMessage = $"Actualizado: {CurrentStats.RamUsagePercent}% RAM usado";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CleanTopRam()
    {
        if (CurrentStats.TopRamProcesses.Count == 0)
        {
            StatusMessage = "Sin procesos para limpiar";
            return;
        }

        var processesToClean = CurrentStats.TopRamProcesses
            .Take(3)
            .Select(p => p.ProcessId)
            .ToList();

        ExecuteCleanup(processesToClean, "Top 3 procesos");
    }

    private bool CanCleanTopRam() => CurrentStats.TopRamProcesses.Count > 0 && !IsLoading;

    private void CleanAll()
    {
        var processesToClean = CurrentStats.TopRamProcesses
            .Select(p => p.ProcessId)
            .ToList();

        ExecuteCleanup(processesToClean, "todos los procesos");
    }

    private void ExecuteCleanup(List<int> processIds, string description)
    {
        if (processIds.Count == 0)
        {
            StatusMessage = "Sin procesos para limpiar";
            HasCleanupResults = false;
            return;
        }

        IsLoading = true;
        StatusMessage = $"Limpiando {description}...";

        var beforeRam = CurrentStats.RamUsedGb ?? 0;
        var processDetails = new List<string>();

        // Capture process details before cleanup
        foreach (var pid in processIds)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                var memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2);
                processDetails.Add($"• {process.ProcessName} (PID {pid}): {memoryMb} MB");
            }
            catch { }
        }

        try
        {
            var success = _diagnosticsService.CleanMemory(processIds);

            if (success)
            {
                StatusMessage = $"✓ Limpieza exitosa de {description}";
                
                // Build detailed summary
                var summary = $"Limpiados {processIds.Count} proceso(s):\n";
                summary += string.Join("\n", processDetails.Take(10));
                if (processDetails.Count > 10)
                {
                    summary += $"\n... y {processDetails.Count - 10} más";
                }

                CleanupSummary = summary;
                HasCleanupResults = true;

                // Refresh after cleanup with a slight delay
                System.Threading.Tasks.Task.Delay(800).ContinueWith(_ =>
                {
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(() =>
                    {
                        Refresh();
                        var afterRam = CurrentStats.RamUsedGb ?? 0;
                        var savedMb = Math.Round((beforeRam - afterRam) * 1024, 2);
                        if (savedMb > 0)
                        {
                            CleanupSummary += $"\n\n💾 Memoria liberada: ~{savedMb} MB";
                        }
                    });
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                StatusMessage = "Error durante la limpieza";
                HasCleanupResults = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            HasCleanupResults = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CleanTempFiles()
    {
        IsLoading = true;
        StatusMessage = "Limpiando archivos temporales...";

        try
        {
            var result = _diagnosticsService.CleanTempFiles();

            if (result.FilesDeleted > 0)
            {
                StatusMessage = $"✓ Limpieza de archivos temporales exitosa: {result.FilesDeleted} archivos eliminados";
                
                // Build detailed summary
                var summary = $"Archivos eliminados: {result.FilesDeleted}\n";
                summary += $"Espacio liberado: {(result.GbFreed > 0 ? $"{result.GbFreed} GB" : $"{result.MbFreed} MB")}\n\n";
                summary += "Archivos eliminados:\n";
                
                foreach (var file in result.DeletedFiles.Take(20))
                {
                    var fileName = Path.GetFileName(file);
                    summary += $"• {fileName}\n";
                }
                
                if (result.DeletedFiles.Count > 20)
                {
                    summary += $"... y {result.DeletedFiles.Count - 20} más";
                }

                CleanupSummary = summary;
                HasCleanupResults = true;
            }
            else
            {
                StatusMessage = "✓ Archivos temporales ya están limpios";
                CleanupSummary = "No se encontraron archivos temporales para eliminar.";
                HasCleanupResults = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al limpiar archivos temp: {ex.Message}";
            HasCleanupResults = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

