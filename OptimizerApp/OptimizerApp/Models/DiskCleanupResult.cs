namespace OptimizerApp.Models;

public sealed class DiskCleanupResult
{
    public string DriveLetter { get; init; } = "";
    public CleanupType CleanupType { get; init; }
    public bool Success { get; init; }
    public long BytesFreed { get; init; }
    public int FilesDeleted { get; init; }
    public string Message { get; init; } = "";
    public DateTime ExecutedAt { get; init; } = DateTime.Now;
    public double GbFreed => BytesFreed / (1024.0 * 1024.0 * 1024.0);
}

public enum CleanupType
{
    TempFiles,
    Trim,
    Defragmentation
}
