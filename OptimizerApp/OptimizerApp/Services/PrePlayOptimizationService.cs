using System.Diagnostics;
using OptimizerApp.Models;

namespace OptimizerApp.Services;

public sealed class PrePlayOptimizationService : IPrePlayOptimizationService
{
    private readonly object _cpuLock = new();
    private DateTimeOffset _lastSample = DateTimeOffset.MinValue;
    private Dictionary<int, TimeSpan> _lastCpuTimes = new();

    private static readonly Dictionary<string, string> AppDescriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["code"] = "Editor de código (Visual Studio Code).",
        ["optimizerapp"] = "OptimizerApp en ejecución.",
        ["discord"] = "Chat de voz y texto para comunidades.",
        ["teams"] = "Comunicación y reuniones.",
        ["chrome"] = "Navegador web.",
        ["msedge"] = "Navegador web.",
        ["firefox"] = "Navegador web.",
        ["spotify"] = "Reproductor de música.",
        ["steam"] = "Cliente de juegos y tienda digital.",
        ["steamwebhelper"] = "Componente web de Steam.",
        ["epicgameslauncher"] = "Cliente de juegos y tienda digital.",
        ["battlenet"] = "Cliente de juegos y actualizaciones.",
        ["riotclientservices"] = "Cliente de juegos y servicios asociados.",
        ["obs64"] = "Grabación y streaming.",
        ["origin"] = "Cliente de juegos y servicios asociados.",
        ["goggalaxy"] = "Cliente de juegos y tienda digital.",
        ["ubisoftconnect"] = "Cliente de juegos y servicios asociados.",
        ["overwolf"] = "Herramientas y overlays para juegos.",
        ["nvidia share"] = "Overlay y captura de NVIDIA.",
        ["amdsoftware"] = "Panel y overlay de AMD.",
        ["msi afterburner"] = "Monitorización y overclocking.",
        ["logitechghub"] = "Control de periféricos gaming.",
        ["razer synapse"] = "Control de periféricos gaming.",
        ["steelseriesgg"] = "Control de periféricos gaming.",
        ["whatsapp"] = "Mensajería de escritorio."
    };

    private static readonly HashSet<string> CriticalProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "explorer",
        "dwm",
        "csrss",
        "winlogon",
        "services",
        "lsass",
        "svchost",
        "smss",
        "spoolsv",
        "System"
    };

    public IReadOnlyList<OptimizationCandidate> GetRunningProcesses()
    {
        var processes = Process.GetProcesses();
        var userProcesses = processes.Where(IsUserProcess).ToArray();
        var cpuPercentById = GetCpuPercentByProcessId(processes);

        var grouped = userProcesses
            .GroupBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Sum(p => SafeGetWorkingSet(p)))
            .ToList();

        var results = new List<OptimizationCandidate>(grouped.Count);

        foreach (var group in grouped)
        {
            var name = group.Key;
            var count = group.Count();
            var memoryMb = Math.Round(group.Sum(p => SafeGetWorkingSet(p)) / 1024d / 1024d, 2);
            var cpuPercent = Math.Round(group.Sum(p => cpuPercentById.GetValueOrDefault(p.Id) ?? 0), 2);
            var cpuText = cpuPercent <= 0 ? "N/D" : $"{cpuPercent}%";
            var savings = $"Ahorro estimado: CPU {cpuText} | RAM {memoryMb} MB";
            var description = AppDescriptions.TryGetValue(name, out var desc)
                ? desc
                : "Aplicación externa al sistema operativo.";

            results.Add(new OptimizationCandidate
            {
                Name = name,
                Reason = "Aplicación externa",
                Description = description,
                SavingsDescription = savings,
                ProcessCount = count,
                MemoryMb = memoryMb,
                CpuPercent = cpuPercent <= 0 ? null : cpuPercent,
                IsCritical = IsCritical(name)
            });
        }

        return results;
    }

    public ProcessCloseResult CloseByName(string processName)
    {
        if (IsCritical(processName))
        {
            return new ProcessCloseResult { Name = processName, ClosedCount = 0, FailedCount = 0 };
        }

        var closed = 0;
        var failed = 0;

        foreach (var process in Process.GetProcessesByName(processName))
        {
            try
            {
                process.Kill(entireProcessTree: true);
                closed++;
            }
            catch
            {
                failed++;
            }
        }

        return new ProcessCloseResult
        {
            Name = processName,
            ClosedCount = closed,
            FailedCount = failed
        };
    }

    private static long SafeGetWorkingSet(Process process)
    {
        try
        {
            return process.WorkingSet64;
        }
        catch
        {
            return 0;
        }
    }

    private static bool IsUserProcess(Process process)
    {
        var path = TryGetProcessPath(process);
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return !path.StartsWith(windowsDir, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryGetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<int, double?> GetCpuPercentByProcessId(Process[] processes)
    {
        var now = DateTimeOffset.UtcNow;
        var currentCpuTimes = new Dictionary<int, TimeSpan>(processes.Length);
        var cpuPercentById = new Dictionary<int, double?>(processes.Length);

        lock (_cpuLock)
        {
            var elapsedSeconds = (_lastSample == DateTimeOffset.MinValue)
                ? 0
                : (now - _lastSample).TotalSeconds;

            foreach (var process in processes)
            {
                try
                {
                    var totalCpu = process.TotalProcessorTime;
                    currentCpuTimes[process.Id] = totalCpu;

                    double? cpuPercent = null;
                    if (elapsedSeconds > 0 && _lastCpuTimes.TryGetValue(process.Id, out var previousCpu))
                    {
                        var delta = totalCpu - previousCpu;
                        var rawPercent = delta.TotalSeconds / elapsedSeconds / Environment.ProcessorCount * 100;
                        cpuPercent = rawPercent >= 0 ? Math.Round(rawPercent, 2) : null;
                    }

                    cpuPercentById[process.Id] = cpuPercent;
                }
                catch
                {
                    // Ignore inaccessible process.
                }
            }

            _lastCpuTimes = currentCpuTimes;
            _lastSample = now;
        }

        return cpuPercentById;
    }

    private static bool IsCritical(string processName)
        => CriticalProcessNames.Contains(processName);
}
