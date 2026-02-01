using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OptimizerApp.Models;

namespace OptimizerApp.Services;

public sealed class SystemDiagnosticsService : ISystemDiagnosticsService
{
    private readonly object _processLock = new();
    private DateTimeOffset _lastProcessSample = DateTimeOffset.MinValue;
    private Dictionary<int, TimeSpan> _lastCpuTimes = new();
    private readonly object _systemCpuLock = new();
    private DateTimeOffset _lastSystemCpuSample = DateTimeOffset.MinValue;
    private TimeSpan _lastSystemCpuTime = TimeSpan.Zero;

    public DiagnosticsSnapshot GetSnapshot()
    {
        var cpuUsage = GetSystemCpuPercent();
        var availableGb = GetAvailableRamGb();

        var totalRamGb = GetTotalRamGb();
        double? ramUsagePercent = totalRamGb is > 0 && availableGb is not null
            ? Math.Round(100 - (availableGb.Value / totalRamGb.Value * 100), 2)
            : null;

        var (diskFreeGb, diskFreePercent) = GetSystemDriveFree();

        var (score, status) = DiagnosticsScoring.Calculate(cpuUsage, ramUsagePercent, diskFreePercent);

        return new DiagnosticsSnapshot
        {
            CpuUsagePercent = cpuUsage,
            RamAvailableGb = availableGb,
            RamUsagePercent = ramUsagePercent,
            DiskFreeGb = diskFreeGb,
            DiskFreePercent = diskFreePercent,
            HealthScore = score,
            HealthStatus = status
        };
    }

    public IReadOnlyList<ProcessUsageInfo> GetTopProcesses(int count)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var processList = Process.GetProcesses();
            var currentCpuTimes = new Dictionary<int, TimeSpan>(processList.Length);
            var cpuPercentById = new Dictionary<int, double?>(processList.Length);

            lock (_processLock)
            {
                var elapsedSeconds = (_lastProcessSample == DateTimeOffset.MinValue)
                    ? 0
                    : (now - _lastProcessSample).TotalSeconds;

                foreach (var process in processList)
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
                        // Access denied or process exited.
                    }
                }

                _lastCpuTimes = currentCpuTimes;
                _lastProcessSample = now;
            }

            var results = new List<ProcessUsageInfo>(processList.Length);
            foreach (var process in processList)
            {
                try
                {
                    var memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2);
                    cpuPercentById.TryGetValue(process.Id, out var cpuPercent);

                    results.Add(new ProcessUsageInfo
                    {
                        ProcessId = process.Id,
                        Name = process.ProcessName,
                        CpuPercent = cpuPercent,
                        MemoryMb = memoryMb
                    });
                }
                catch
                {
                    // Skip inaccessible processes.
                }
            }

            return results
                .OrderByDescending(p => p.CpuPercent ?? -1)
                .ThenByDescending(p => p.MemoryMb)
                .Take(Math.Clamp(count, 1, 50))
                .ToList();
        }
        catch
        {
            return Array.Empty<ProcessUsageInfo>();
        }
    }

    public IReadOnlyList<StartupAppInfo> GetStartupApps()
    {
        var apps = new List<StartupAppInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var locations = new (RegistryHive hive, RegistryView view, string path)[]
        {
            (RegistryHive.CurrentUser, RegistryView.Registry64, @"Software\Microsoft\Windows\CurrentVersion\Run"),
            (RegistryHive.CurrentUser, RegistryView.Registry32, @"Software\Microsoft\Windows\CurrentVersion\Run"),
            (RegistryHive.LocalMachine, RegistryView.Registry64, @"Software\Microsoft\Windows\CurrentVersion\Run"),
            (RegistryHive.LocalMachine, RegistryView.Registry32, @"Software\Microsoft\Windows\CurrentVersion\Run")
        };

        foreach (var (hive, view, path) in locations)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var key = baseKey.OpenSubKey(path);
                if (key is null)
                {
                    continue;
                }

                foreach (var name in key.GetValueNames())
                {
                    var command = key.GetValue(name)?.ToString() ?? string.Empty;
                    var identifier = $"{hive}-{view}-{name}-{command}";
                    if (!seen.Add(identifier))
                    {
                        continue;
                    }

                    apps.Add(new StartupAppInfo
                    {
                        Name = name,
                        Command = command,
                        Location = $"{hive} ({view})"
                    });
                }
            }
            catch
            {
                // Ignore registry access issues.
            }
        }

        return apps.OrderBy(a => a.Name).ToList();
    }

    private double? GetSystemCpuPercent()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var totalCpu = GetTotalCpuTime();

            lock (_systemCpuLock)
            {
                if (_lastSystemCpuSample == DateTimeOffset.MinValue)
                {
                    _lastSystemCpuSample = now;
                    _lastSystemCpuTime = totalCpu;
                    return null;
                }

                var elapsedSeconds = (now - _lastSystemCpuSample).TotalSeconds;
                if (elapsedSeconds <= 0)
                {
                    return null;
                }

                var delta = totalCpu - _lastSystemCpuTime;
                var rawPercent = delta.TotalSeconds / elapsedSeconds / Environment.ProcessorCount * 100;
                _lastSystemCpuSample = now;
                _lastSystemCpuTime = totalCpu;

                return rawPercent >= 0 ? Math.Round(rawPercent, 2) : null;
            }
        }
        catch
        {
            return null;
        }
    }

    private static TimeSpan GetTotalCpuTime()
    {
        var total = TimeSpan.Zero;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                total += process.TotalProcessorTime;
            }
            catch
            {
                // Ignore processes we can't access.
            }
        }

        return total;
    }

    private static double? GetTotalRamGb()
    {
        try
        {
            var status = new MemoryStatusEx();
            return GlobalMemoryStatusEx(ref status)
                ? Math.Round(status.ullTotalPhys / 1024d / 1024d / 1024d, 2)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static double? GetAvailableRamGb()
    {
        try
        {
            var status = new MemoryStatusEx();
            return GlobalMemoryStatusEx(ref status)
                ? Math.Round(status.ullAvailPhys / 1024d / 1024d / 1024d, 2)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static (double? freeGb, double? freePercent) GetSystemDriveFree()
    {
        try
        {
            var systemDrive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.IsReady && d.RootDirectory.FullName.Equals(Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase));

            if (systemDrive is null)
            {
                return (null, null);
            }

            var freeGb = Math.Round(systemDrive.AvailableFreeSpace / 1024d / 1024d / 1024d, 2);
            var freePercent = Math.Round(systemDrive.AvailableFreeSpace / (double)systemDrive.TotalSize * 100, 2);
            return (freeGb, freePercent);
        }
        catch
        {
            return (null, null);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MemoryStatusEx()
        {
            dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>();
            dwMemoryLoad = 0;
            ullTotalPhys = 0;
            ullAvailPhys = 0;
            ullTotalPageFile = 0;
            ullAvailPageFile = 0;
            ullTotalVirtual = 0;
            ullAvailVirtual = 0;
            ullAvailExtendedVirtual = 0;
        }
    }

    public MemoryStats GetMemoryStats(int topProcessCount = 10)
    {
        try
        {
            var cpuUsage = GetSystemCpuPercent();
            var availableGb = GetAvailableRamGb();
            var totalRamGb = GetTotalRamGb();
            
            double? usedRamGb = null;
            double? ramUsagePercent = null;
            
            if (totalRamGb.HasValue && availableGb.HasValue)
            {
                usedRamGb = Math.Round(totalRamGb.Value - availableGb.Value, 2);
                ramUsagePercent = Math.Round((usedRamGb.Value / totalRamGb.Value) * 100, 2);
            }

            var topProcesses = GetTopProcesses(topProcessCount);
            var topRamProcesses = topProcesses.OrderByDescending(p => p.MemoryMb).ToList();
            var topCpuProcesses = topProcesses.OrderByDescending(p => p.CpuPercent ?? -1).ToList();

            // Get Chrome processes
            var chromeProcesses = Process.GetProcessesByName("chrome");
            var chromeStats = new List<ProcessUsageInfo>();
            double totalChromeRamGb = 0;

            foreach (var process in chromeProcesses)
            {
                try
                {
                    var memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2);
                    var cpuPercent = topProcesses.FirstOrDefault(p => p.ProcessId == process.Id)?.CpuPercent;

                    chromeStats.Add(new ProcessUsageInfo
                    {
                        ProcessId = process.Id,
                        Name = process.ProcessName,
                        CpuPercent = cpuPercent,
                        MemoryMb = memoryMb
                    });

                    totalChromeRamGb += memoryMb;
                }
                catch
                {
                    // Skip inaccessible processes.
                }
            }

            totalChromeRamGb = Math.Round(totalChromeRamGb / 1024d, 2);
            chromeStats = chromeStats.OrderByDescending(p => p.MemoryMb).ToList();

            return new MemoryStats
            {
                CpuUsagePercent = cpuUsage,
                RamUsedGb = usedRamGb,
                RamTotalGb = totalRamGb,
                RamUsagePercent = ramUsagePercent,
                ChromeProcessCount = chromeProcesses.Length,
                ChromeRamGb = totalChromeRamGb,
                TopRamProcesses = topRamProcesses,
                TopCpuProcesses = topCpuProcesses,
                ChromeProcesses = chromeStats
            };
        }
        catch
        {
            return new MemoryStats();
        }
    }

    public bool CleanMemory(IReadOnlyList<int> processIds)
    {
        try
        {
            foreach (var processId in processIds)
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    if (!process.HasExited)
                    {
                        // Try to empty working set (reduces memory footprint)
                        EmptyWorkingSet(process.Handle);
                    }
                }
                catch
                {
                    // Skip processes that can't be accessed or don't exist
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);

    private static void EmptyWorkingSet(IntPtr processHandle)
    {
        SetProcessWorkingSetSize(processHandle, new IntPtr(-1), new IntPtr(-1));
    }

    public TempFileCleanupResult CleanTempFiles()
    {
        var result = new TempFileCleanupResult();
        var tempPaths = new List<string>
        {
            Environment.GetEnvironmentVariable("TEMP") ?? "",
            Environment.GetEnvironmentVariable("TMP") ?? "",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Local\Temp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Temp"),
        };

        foreach (var tempPath in tempPaths.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p)))
        {
            try
            {
                var directory = new DirectoryInfo(tempPath);
                
                // Delete files
                foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        result.BytesFreed += file.Length;
                        file.Delete();
                        result.FilesDeleted++;
                        result.DeletedFiles.Add(file.FullName);
                    }
                    catch
                    {
                        // Skip files that can't be deleted (in use, permission denied, etc.)
                    }
                }

                // Delete empty directories
                foreach (var subDir in directory.GetDirectories("*", SearchOption.AllDirectories).OrderByDescending(d => d.FullName.Length))
                {
                    try
                    {
                        if (subDir.GetFiles().Length == 0 && subDir.GetDirectories().Length == 0)
                        {
                            subDir.Delete();
                        }
                    }
                    catch
                    {
                        // Skip directories that can't be deleted
                    }
                }
            }
            catch
            {
                // Skip temp paths that can't be accessed
            }
        }

        return result;
    }
}

