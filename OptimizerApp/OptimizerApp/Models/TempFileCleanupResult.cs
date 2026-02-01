namespace OptimizerApp.Models;

public sealed class TempFileCleanupResult
{
    public int FilesDeleted { get; set; }
    public long BytesFreed { get; set; }
    public List<string> DeletedFiles { get; set; } = new();

    public double MbFreed => Math.Round(BytesFreed / 1024d / 1024d, 2);
    public double GbFreed => Math.Round(BytesFreed / 1024d / 1024d / 1024d, 2);

    public string Summary => $"Archivos eliminados: {FilesDeleted} | Espacio liberado: {(GbFreed > 0 ? $"{GbFreed} GB" : $"{MbFreed} MB")}";
}
