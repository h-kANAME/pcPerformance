using OptimizerApp.Models;

namespace OptimizerApp.Services;

public interface IDiskOptimizationService
{
    /// <summary>
    /// Obtiene la lista de unidades detectadas en el sistema
    /// </summary>
    IReadOnlyList<DriveInfo> GetDrives();

    /// <summary>
    /// Obtiene información detallada de una unidad específica
    /// </summary>
    DriveInfo? GetDriveInfo(string driveLetter);

    /// <summary>
    /// Limpia archivos temporales en una unidad
    /// </summary>
    DiskOptimizationReport CleanTempFiles(string driveLetter);

    /// <summary>
    /// Ejecuta TRIM en una unidad (SSD optimization)
    /// </summary>
    DiskOptimizationReport ExecuteTrim(string driveLetter);

    /// <summary>
    /// Desfragmenta una unidad
    /// </summary>
    DiskOptimizationReport Defragment(string driveLetter);

    /// <summary>
    /// Obtiene el tipo de unidad (HDD, SSD, etc)
    /// </summary>
    string GetDriveType(string driveLetter);
}
