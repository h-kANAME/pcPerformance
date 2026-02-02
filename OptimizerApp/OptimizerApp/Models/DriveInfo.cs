namespace OptimizerApp.Models;

public sealed class DriveInfo
{
    public string DriveLetter { get; init; } = "";
    public string Name { get; init; } = "";
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
    public long UsedBytes => TotalBytes - FreeBytes;
    public double UsagePercentage => TotalBytes > 0 ? (UsedBytes * 100.0) / TotalBytes : 0;
    public double TotalGb => TotalBytes / (1024.0 * 1024.0 * 1024.0);
    public double FreeGb => FreeBytes / (1024.0 * 1024.0 * 1024.0);
    public double UsedGb => UsedBytes / (1024.0 * 1024.0 * 1024.0);
    public DriveType DriveType { get; init; }
    public bool IsReady { get; init; }
}

public enum DriveType
{
    Unknown = 0,
    NoRootDirectory = 1,
    RemovableDisk = 2,
    FixedDisk = 3,
    Network = 4,
    CDRom = 5,
    Ram = 6
}
