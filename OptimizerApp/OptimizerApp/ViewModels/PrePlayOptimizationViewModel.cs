using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using OptimizerApp.Models;
using OptimizerApp.Services;
using OptimizerApp.Views;

namespace OptimizerApp.ViewModels;

public sealed class PrePlayOptimizationViewModel : ObservableObject
{
    private readonly ISystemDiagnosticsService _diagnosticsService;
    private readonly IPrePlayOptimizationService _optimizationService;


    private DiagnosticsSnapshot _before = new();
    private DiagnosticsSnapshot _after = new();
    private string _lastRunSummary = "";

    public DiagnosticsSnapshot Before
    {
        get => _before;
        private set => SetProperty(ref _before, value);
    }

    public DiagnosticsSnapshot After
    {
        get => _after;
        private set => SetProperty(ref _after, value);
    }

    public string LastRunSummary
    {
        get => _lastRunSummary;
        private set => SetProperty(ref _lastRunSummary, value);
    }

    public ObservableCollection<OptimizationCandidate> Candidates { get; } = new();

    public RelayCommand AnalyzeCommand { get; }
    public RelayCommand OptimizeCommand { get; }
    public RelayCommand OptimizeAllCommand { get; }
    public RelayCommand<OptimizationCandidate> CloseProcessCommand { get; }

    public PrePlayOptimizationViewModel(
        ISystemDiagnosticsService diagnosticsService,
        IPrePlayOptimizationService optimizationService)
    {
        _diagnosticsService = diagnosticsService;
        _optimizationService = optimizationService;

        AnalyzeCommand = new RelayCommand(Analyze);
        OptimizeCommand = new RelayCommand(() => Optimize(forceAll: false));
        OptimizeAllCommand = new RelayCommand(() => Optimize(forceAll: true));
        CloseProcessCommand = new RelayCommand<OptimizationCandidate>(CloseProcess);

        Analyze();
    }

    private void Analyze()
    {
        RefreshCandidates();
        Before = _diagnosticsService.GetSnapshot();
        LastRunSummary = "";
    }

    private void RefreshCandidates()
    {
        Task.Run(() =>
        {
            try
            {
                var candidates = _optimizationService.GetRunningProcesses();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Candidates.Clear();
                    foreach (var candidate in candidates)
                    {
                        Candidates.Add(candidate);
                    }
                });
            }
            catch (Exception ex)
            {
                // Log silenciosamente los errores de acceso a procesos
                System.Diagnostics.Debug.WriteLine($"Error en RefreshCandidates: {ex.Message}");
            }
        });
    }

    private void CloseProcess(OptimizationCandidate? candidate)
    {
        if (candidate == null || !candidate.IsRunning)
            return;

        if (candidate.IsCritical)
        {
            MessageBox.Show(
                $"No se puede cerrar '{candidate.Name}' porque es un proceso crítico del sistema.",
                "Proceso Crítico",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var message = $"{candidate.Name}\n{candidate.Description}\n{candidate.SavingsDescription}\n\n¿Deseas cerrar esta aplicación?";
        var decision = ConfirmCloseDialog.ShowDialog(Application.Current.MainWindow!, "Confirmación", message);

        if (decision != CloseDecision.Close)
            return;

        var result = _optimizationService.CloseByName(candidate.Name);
        
        var summary = $"Cerrado: {result.ClosedCount} | Fallos: {result.FailedCount}";
        if (result.FailedCount > 0)
        {
            MessageBox.Show(summary, "Resultado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        RefreshCandidates();
    }

    private void Optimize(bool forceAll)
    {
        var runningCandidates = Candidates.Where(c => c.IsRunning).ToList();
        if (runningCandidates.Count == 0)
        {
            MessageBox.Show("No hay procesos candidatos en ejecución.", "Optimización", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (forceAll)
        {
            var confirm = MessageBox.Show(
                "Se intentará cerrar todos los procesos candidatos (excepto procesos críticos del sistema). ¿Continuar?",
                "Optimización",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                forceAll = false;
            }
        }

        var before = _diagnosticsService.GetSnapshot();
        var closed = new List<ProcessCloseResult>();
        var skipped = new List<string>();

        var stopRequested = false;

        foreach (var candidate in runningCandidates)
        {
            if (candidate.IsCritical)
            {
                skipped.Add(candidate.Name);
                continue;
            }

            var shouldClose = forceAll;
            if (!forceAll)
            {
                var message = $"{candidate.Name}\n{candidate.Description}\n{candidate.SavingsDescription}\n\n¿Deseas cerrar esta aplicación?";
                var decision = ConfirmCloseDialog.ShowDialog(Application.Current.MainWindow!, "Confirmación", message);

                if (decision == CloseDecision.Stop)
                {
                    stopRequested = true;
                    break;
                }

                shouldClose = decision == CloseDecision.Close;
            }

            if (!shouldClose)
            {
                skipped.Add(candidate.Name);
                continue;
            }

            closed.Add(_optimizationService.CloseByName(candidate.Name));
            RefreshCandidates();
        }

        var after = _diagnosticsService.GetSnapshot();
        Before = before;
        After = after;
        LastRunSummary = BuildSummary(before, after, closed, skipped, stopRequested);

        RefreshCandidates();
    }


    private static string BuildSummary(
        DiagnosticsSnapshot before,
        DiagnosticsSnapshot after,
        IReadOnlyList<ProcessCloseResult> closed,
        IReadOnlyList<string> skipped,
        bool stopRequested)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Antes: CPU {before.CpuUsagePercent}% | RAM {before.RamUsagePercent}% | Disco libre {before.DiskFreePercent}%");
        builder.AppendLine($"Después: CPU {after.CpuUsagePercent}% | RAM {after.RamUsagePercent}% | Disco libre {after.DiskFreePercent}%");
        builder.AppendLine();

        foreach (var result in closed)
        {
            builder.AppendLine($"Cerrado {result.Name}: OK {result.ClosedCount}, Fallos {result.FailedCount}");
        }

        if (skipped.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine($"Omitidos: {string.Join(", ", skipped)}");
        }

        if (stopRequested)
        {
            builder.AppendLine();
            builder.AppendLine("Proceso detenido por el usuario (Continuar después).");
        }

        return builder.ToString().Trim();
    }
}
