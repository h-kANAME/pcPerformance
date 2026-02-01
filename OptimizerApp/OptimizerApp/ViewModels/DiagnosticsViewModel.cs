using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using OptimizerApp.Models;
using OptimizerApp.Services;

namespace OptimizerApp.ViewModels;

public sealed class DiagnosticsViewModel : ObservableObject, IDisposable
{
    private readonly ISystemDiagnosticsService _service;
    private readonly DispatcherTimer _timer;

    private DiagnosticsSnapshot _snapshot = new();
    private int _intervalSeconds = 3;

    public DiagnosticsSnapshot Snapshot
    {
        get => _snapshot;
        private set => SetProperty(ref _snapshot, value);
    }

    public int IntervalSeconds
    {
        get => _intervalSeconds;
        set
        {
            if (SetProperty(ref _intervalSeconds, value))
            {
                _timer.Interval = TimeSpan.FromSeconds(Math.Clamp(value, 1, 30));
            }
        }
    }

    public ObservableCollection<Recommendation> Recommendations { get; } = new();
    public ObservableCollection<ProcessUsageInfo> TopProcesses { get; } = new();
    public ObservableCollection<StartupAppInfo> StartupApps { get; } = new();

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ExportJsonCommand { get; }
    public RelayCommand ExportCsvCommand { get; }
    public RelayCommand OpenStartupAppsCommand { get; }
    public RelayCommand OpenStorageSenseCommand { get; }

    public DiagnosticsViewModel(ISystemDiagnosticsService service)
    {
        _service = service;

        RefreshCommand = new RelayCommand(UpdateSnapshot);
        ExportJsonCommand = new RelayCommand(ExportJsonReport);
        ExportCsvCommand = new RelayCommand(ExportCsvReport);
        OpenStartupAppsCommand = new RelayCommand(() => OpenSettings("ms-settings:startupapps"));
        OpenStorageSenseCommand = new RelayCommand(() => OpenSettings("ms-settings:storagesense"));

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_intervalSeconds)
        };
        _timer.Tick += (_, _) => UpdateSnapshot();
        _timer.Start();

        UpdateSnapshot();
    }

    public void Dispose()
    {
        _timer.Stop();
    }

    private void UpdateSnapshot()
    {
        Snapshot = _service.GetSnapshot();
        UpdateTopProcesses();
        UpdateStartupApps();
        BuildRecommendations();
    }

    private void UpdateTopProcesses()
    {
        TopProcesses.Clear();
        foreach (var process in _service.GetTopProcesses(5))
        {
            TopProcesses.Add(process);
        }
    }

    private void UpdateStartupApps()
    {
        StartupApps.Clear();
        foreach (var app in _service.GetStartupApps())
        {
            StartupApps.Add(app);
        }
    }

    private void BuildRecommendations()
    {
        Recommendations.Clear();

        if (Snapshot.CpuUsagePercent is >= 80)
        {
            Recommendations.Add(new Recommendation
            {
                Title = "CPU alta",
                Description = "Hay uso elevado de CPU. Considera cerrar procesos de alto consumo.",
                Severity = HealthStatus.Warning
            });
        }

        if (Snapshot.RamUsagePercent is >= 85)
        {
            Recommendations.Add(new Recommendation
            {
                Title = "Memoria alta",
                Description = "RAM casi al límite. Cierra aplicaciones que no uses.",
                Severity = HealthStatus.Warning
            });
        }

        if (Snapshot.DiskFreePercent is <= 15)
        {
            Recommendations.Add(new Recommendation
            {
                Title = "Poco espacio en disco",
                Description = "Libera espacio con Storage Sense o elimina archivos grandes.",
                Severity = HealthStatus.Warning
            });
        }

        if (Recommendations.Count == 0)
        {
            Recommendations.Add(new Recommendation
            {
                Title = "Todo en orden",
                Description = "No se detectaron problemas críticos.",
                Severity = HealthStatus.Ok
            });
        }
    }

    private void ExportJsonReport()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            FileName = $"diagnostico_{DateTimeOffset.Now:yyyyMMdd_HHmm}.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var payload = new
        {
            Snapshot.Timestamp,
            Snapshot.CpuUsagePercent,
            Snapshot.RamUsagePercent,
            Snapshot.RamAvailableGb,
            Snapshot.DiskFreePercent,
            Snapshot.DiskFreeGb,
            Snapshot.HealthScore,
            Snapshot.HealthStatus,
            Recommendations,
            TopProcesses,
            StartupApps
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(dialog.FileName, json);
        MessageBox.Show("Reporte JSON exportado.", "Diagnóstico", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportCsvReport()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"diagnostico_{DateTimeOffset.Now:yyyyMMdd_HHmm}.csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var lines = new List<string>
        {
            "Timestamp,CpuUsagePercent,RamUsagePercent,RamAvailableGb,DiskFreePercent,DiskFreeGb,HealthScore,HealthStatus",
            $"{Snapshot.Timestamp:o},{Snapshot.CpuUsagePercent},{Snapshot.RamUsagePercent},{Snapshot.RamAvailableGb},{Snapshot.DiskFreePercent},{Snapshot.DiskFreeGb},{Snapshot.HealthScore},{Snapshot.HealthStatus}"
        };

        File.WriteAllLines(dialog.FileName, lines);
        MessageBox.Show("Reporte CSV exportado.", "Diagnóstico", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void OpenSettings(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo abrir la configuración: {ex.Message}", "Diagnóstico", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
