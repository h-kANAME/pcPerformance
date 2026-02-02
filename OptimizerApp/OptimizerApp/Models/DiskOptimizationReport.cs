namespace OptimizerApp.Models;

public sealed class DiskOptimizationReport
{
    public string DriveLetter { get; init; } = "";
    public CleanupType OperationType { get; init; }
    public bool Success { get; init; }
    public long BytesFreed { get; init; }
    public double GbFreed => BytesFreed / (1024.0 * 1024.0 * 1024.0);
    public int FileCount { get; init; }
    public TimeSpan Duration { get; init; }
    public string Description { get; init; } = "";
    public IReadOnlyList<string> Benefits { get; init; } = [];
    public DateTime ExecutedAt { get; init; } = DateTime.Now;
    
    public string FormattedResult => OperationType switch
    {
        CleanupType.TempFiles => $@"
═══════════════════════════════════════
Unidad: {DriveLetter}
Operación: {GetOperationName()}
Estado: {(Success ? "✓ Completado" : "✗ Error")}
═══════════════════════════════════════

Espacio liberado: {GbFreed:F2} GB ({BytesFreed:N0} bytes)
Archivos eliminados: {FileCount}
Duración: {Duration.TotalSeconds:F1} segundos

{Description}

Ventajas del sistema:
{string.Join("\n", Benefits.Select((b, i) => $"{i + 1}. {b}"))}

═══════════════════════════════════════",

        CleanupType.Trim => $@"
═══════════════════════════════════════
Unidad: {DriveLetter}
Operación: {GetOperationName()}
Estado: {(Success ? "✓ Completado" : "✗ Error")}
═══════════════════════════════════════

Duración: {Duration.TotalSeconds:F1} segundos

{Description}

Beneficios de la optimización:
{string.Join("\n", Benefits.Select((b, i) => $"{i + 1}. {b}"))}

═══════════════════════════════════════",

        CleanupType.Defragmentation => $@"
═══════════════════════════════════════
Unidad: {DriveLetter}
Operación: {GetOperationName()}
Estado: {(Success ? "✓ Completado" : "✗ Error")}
═══════════════════════════════════════

Duración: {Duration.TotalSeconds:F1} segundos
Espacio procesado: {BytesFreed:N0} bytes ({GbFreed:F2} GB)

{Description}

Beneficios de la desfragmentación:
{string.Join("\n", Benefits.Select((b, i) => $"{i + 1}. {b}"))}

═══════════════════════════════════════",

        _ => $@"
═══════════════════════════════════════
Unidad: {DriveLetter}
Operación: {GetOperationName()}
Estado: {(Success ? "✓ Completado" : "✗ Error")}
═══════════════════════════════════════

Espacio liberado: {GbFreed:F2} GB ({BytesFreed:N0} bytes)
Archivos procesados: {FileCount}
Duración: {Duration.TotalSeconds:F1} segundos

{Description}

Ventajas del sistema:
{string.Join("\n", Benefits.Select((b, i) => $"{i + 1}. {b}"))}

═══════════════════════════════════════"
    };


    private string GetOperationName() => OperationType switch
    {
        CleanupType.TempFiles => "Limpieza de Archivos Temporales",
        CleanupType.Trim => "TRIM (Optimización de SSD)",
        CleanupType.Defragmentation => "Desfragmentación",
        _ => "Desconocida"
    };
}
