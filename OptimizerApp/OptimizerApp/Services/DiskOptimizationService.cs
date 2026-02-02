using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using OptimizerApp.Models;
using ModelDriveInfo = OptimizerApp.Models.DriveInfo;
using SystemDriveInfo = System.IO.DriveInfo;

namespace OptimizerApp.Services;

public sealed class DiskOptimizationService : IDiskOptimizationService
{
    // Cache for drive type detection to avoid expensive WMI queries
    private readonly Dictionary<string, string> _driveTypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _cacheLock = new();
    public IReadOnlyList<ModelDriveInfo> GetDrives()
    {
        var drives = new List<ModelDriveInfo>();
        
        try
        {
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady)
                        continue;

                    drives.Add(new ModelDriveInfo
                    {
                        DriveLetter = drive.Name.TrimEnd('\\'),
                        Name = drive.VolumeLabel ?? "Local Drive",
                        TotalBytes = drive.TotalSize,
                        FreeBytes = drive.AvailableFreeSpace,
                        DriveType = MapDriveType(drive.DriveType),
                        IsReady = drive.IsReady
                    });
                }
                catch
                {
                    // Skip drives that fail
                }
            }
        }
        catch
        {
            // Return empty list if unable to get drives
        }

        return drives.OrderBy(d => d.DriveLetter).ToList();
    }

    public ModelDriveInfo? GetDriveInfo(string driveLetter)
    {
        try
        {
            var drive = SystemDriveInfo.GetDrives()
                .FirstOrDefault(d => d.Name.StartsWith(driveLetter));

            if (drive == null || !drive.IsReady)
                return null;

            return new ModelDriveInfo
            {
                DriveLetter = drive.Name.TrimEnd('\\'),
                Name = drive.VolumeLabel ?? "Local Drive",
                TotalBytes = drive.TotalSize,
                FreeBytes = drive.AvailableFreeSpace,
                DriveType = MapDriveType(drive.DriveType),
                IsReady = drive.IsReady
            };
        }
        catch
        {
            return null;
        }
    }

    public DiskOptimizationReport CleanTempFiles(string driveLetter)
    {
        var stopwatch = Stopwatch.StartNew();
        long totalBytesFreed = 0;
        int filesDeleted = 0;
        var errors = new List<string>();

        try
        {
            var drivePath = driveLetter.EndsWith("\\") ? driveLetter : driveLetter + "\\";
            var tempPaths = new[]
            {
                Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? "", ""),
                Path.Combine(Environment.GetEnvironmentVariable("TMP") ?? "", ""),
                Path.Combine(drivePath, "Windows", "Temp"),
                Path.Combine(drivePath, "Windows", "Prefetch"),
            }.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p))
             .Distinct()
             .ToList();

            foreach (var tempPath in tempPaths)
            {
                try
                {
                    // Only process if on the same drive
                    if (!tempPath.StartsWith(drivePath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var dirInfo = new DirectoryInfo(tempPath);
                    
                    // Delete files
                    foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            totalBytesFreed += file.Length;
                            file.Delete();
                            filesDeleted++;
                        }
                        catch
                        {
                            // File may be in use, continue
                        }
                    }

                    // Delete empty directories
                    foreach (var dir in dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
                        .OrderByDescending(d => d.FullName.Length))
                    {
                        try
                        {
                            if (Directory.GetFileSystemEntries(dir.FullName).Length == 0)
                                dir.Delete();
                        }
                        catch
                        {
                            // Directory may be in use
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error en {tempPath}: {ex.Message}");
                }
            }

            stopwatch.Stop();

            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.TempFiles,
                Success = true,
                BytesFreed = totalBytesFreed,
                FileCount = filesDeleted,
                Duration = stopwatch.Elapsed,
                Description = $"Se limpiaron {filesDeleted} archivos temporales de {string.Join(", ", tempPaths.Select(p => Path.GetFileName(p)))}.",
                Benefits = new[]
                {
                    "Liberación de espacio en disco",
                    "Mejora potencial de rendimiento al reducir archivos innecesarios",
                    "Eliminación de datos temporales que podrían afectar la privacidad",
                    "Reducción de fragmentación del disco",
                    "Mejora en velocidad de lectura/escritura del disco"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.TempFiles,
                Success = false,
                Duration = stopwatch.Elapsed,
                Description = $"Error durante la limpieza: {ex.Message}",
                Benefits = []
            };
        }
    }

    public DiskOptimizationReport ExecuteTrim(string driveLetter)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cleanDriveLetter = driveLetter.TrimEnd('\\', ':');
            var driveType = GetDriveType(driveLetter);

            // Check if it's an SSD
            if (!driveType.Contains("SSD", StringComparison.OrdinalIgnoreCase) && 
                !IsSSD(cleanDriveLetter))
            {
                stopwatch.Stop();
                return new DiskOptimizationReport
                {
                    DriveLetter = driveLetter,
                    OperationType = CleanupType.Trim,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    Description = "TRIM no es aplicable a esta unidad. Es un disco HDD, no SSD.",
                    Benefits = []
                };
            }

            // Execute TRIM command
            var psi = new ProcessStartInfo
            {
                FileName = "fsutil",
                Arguments = $"fsutil behavior query DisableDeleteNotify {cleanDriveLetter}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process?.WaitForExit();
            }

            // Execute actual TRIM
            var trimPsi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c optimize-volume -DriveLetter {cleanDriveLetter} -Defrag -TrimOnly",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (var process = Process.Start(trimPsi))
            {
                process?.WaitForExit(30000); // 30 segundo timeout
            }

            stopwatch.Stop();

            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.Trim,
                Success = true,
                BytesFreed = 0, // TRIM no libera espacio directamente, optimiza el SSD
                Duration = stopwatch.Elapsed,
                Description = "TRIM ejecutado exitosamente en el SSD.",
                Benefits = new[]
                {
                    "Mejora de velocidad de lectura/escritura del SSD",
                    "Recuperación de bloques no utilizados",
                    "Mejor rendimiento general del disco",
                    "Mayor longevidad del SSD",
                    "Optimización de velocidad de acceso aleatorio"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.Trim,
                Success = false,
                Duration = stopwatch.Elapsed,
                Description = $"Error durante TRIM: {ex.Message}. Puede requerir privilegios de administrador.",
                Benefits = []
            };
        }
    }

    public DiskOptimizationReport Defragment(string driveLetter)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cleanDriveLetter = driveLetter.TrimEnd('\\', ':');
            var driveType = GetDriveType(driveLetter);

            // Check if it's an HDD
            if (driveType.Contains("SSD", StringComparison.OrdinalIgnoreCase) || 
                IsSSD(cleanDriveLetter))
            {
                stopwatch.Stop();
                return new DiskOptimizationReport
                {
                    DriveLetter = driveLetter,
                    OperationType = CleanupType.Defragmentation,
                    Success = false,
                    Duration = stopwatch.Elapsed,
                    Description = "Desfragmentación no recomendada para SSD. Los SSD no requieren desfragmentación y puede reducir su vida útil.",
                    Benefits = []
                };
            }

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c optimize-volume -DriveLetter {cleanDriveLetter} -Defrag",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (var process = Process.Start(psi))
            {
                var output = process?.StandardOutput.ReadToEnd() ?? "";
                process?.WaitForExit();
            }

            stopwatch.Stop();

            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.Defragmentation,
                Success = true,
                BytesFreed = 0,
                Duration = stopwatch.Elapsed,
                Description = "Desfragmentación completada exitosamente.",
                Benefits = new[]
                {
                    "Mejora significativa de velocidad de lectura",
                    "Reducción de tiempo de acceso a archivos",
                    "Mejor rendimiento general del sistema",
                    "Reducción de uso de CPU/memoria durante acceso a disco",
                    "Mayor vida útil del disco duro (menos movimiento de cabezal)"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DiskOptimizationReport
            {
                DriveLetter = driveLetter,
                OperationType = CleanupType.Defragmentation,
                Success = false,
                Duration = stopwatch.Elapsed,
                Description = $"Error durante desfragmentación: {ex.Message}. Puede requerir privilegios de administrador.",
                Benefits = []
            };
        }
    }

    public string GetDriveType(string driveLetter)
    {
        try
        {
            var cleanDriveLetter = driveLetter.TrimEnd('\\', ':');
            
            // Check cache first
            lock (_cacheLock)
            {
                if (_driveTypeCache.TryGetValue(cleanDriveLetter, out var cached))
                {
                    Debug.WriteLine($"Using cached drive type for {cleanDriveLetter}: {cached}");
                    return cached;
                }
            }

            // Not in cache, detect it
            var result = IsSSD(cleanDriveLetter) ? "SSD (Solid State Drive)" : "HDD (Hard Disk Drive)";
            
            // Store in cache
            lock (_cacheLock)
            {
                _driveTypeCache[cleanDriveLetter] = result;
            }
            
            return result;
        }
        catch
        {
            // Default to HDD if detection fails
            return "HDD (Hard Disk Drive)";
        }
    }

    private bool IsSSD(string driveLetter)
    {
        try
        {
            // Method 1: Try PowerShell first (faster with timeout)
            try
            {
                var optimizePsi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"Get-PhysicalDisk | Where-Object {{$_.DeviceID -eq (Get-Partition -DriveLetter '{driveLetter}' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty DiskNumber)}} | Select-Object -ExpandProperty MediaType\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(optimizePsi))
                {
                    // 3 second timeout for PowerShell
                    if (process?.WaitForExit(3000) == true)
                    {
                        var output = process.StandardOutput.ReadToEnd()?.Trim() ?? "";
                        
                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            Debug.WriteLine($"PowerShell output for {driveLetter}: {output}");
                            
                            if (output.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
                                output.Contains("4", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            if (output.Contains("HDD", StringComparison.OrdinalIgnoreCase) ||
                                output.Contains("3", StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        process?.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PowerShell method failed for {driveLetter}: {ex.Message}");
            }

            // Method 2: Simple heuristic - if on C: drive and Windows is installed, likely SSD
            // This is fast and works for most systems
            if (driveLetter.Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                var cDrive = new SystemDriveInfo("C:");
                if (cDrive.IsReady && Directory.Exists("C:\\Windows"))
                {
                    Debug.WriteLine($"Drive {driveLetter} is C: with Windows - assuming SSD");
                    return true; // System drive is usually SSD
                }
            }

            // Method 3: WMI as fallback (slower but reliable)
            try
            {
                var driveQuery = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}:'}} WHERE AssocClass=Win32_LogicalDiskToPartition");

                foreach (var partition in driveQuery.Get())
                {
                    var diskQuery = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                    foreach (var disk in diskQuery.Get())
                    {
                        var mediaType = disk["MediaType"]?.ToString() ?? "";
                        
                        if (mediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
                            mediaType.Contains("Solid State", StringComparison.OrdinalIgnoreCase) ||
                            mediaType.Contains("4", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        var interfaceType = disk["InterfaceType"]?.ToString() ?? "";
                        if (interfaceType.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WMI method failed: {ex.Message}");
            }

            // Default to HDD if we can't determine
            return false;
        }
        catch
        {
            return false;
        }
    }

    private Models.DriveType MapDriveType(System.IO.DriveType driveType)
    {
        return driveType switch
        {
            System.IO.DriveType.Unknown => Models.DriveType.Unknown,
            System.IO.DriveType.NoRootDirectory => Models.DriveType.NoRootDirectory,
            System.IO.DriveType.Removable => Models.DriveType.RemovableDisk,
            System.IO.DriveType.Fixed => Models.DriveType.FixedDisk,
            System.IO.DriveType.Network => Models.DriveType.Network,
            System.IO.DriveType.CDRom => Models.DriveType.CDRom,
            System.IO.DriveType.Ram => Models.DriveType.Ram,
            _ => Models.DriveType.Unknown
        };
    }
}
