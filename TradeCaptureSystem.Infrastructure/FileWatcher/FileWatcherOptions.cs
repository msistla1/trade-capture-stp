namespace TradeCaptureSystem.Infrastructure.FileWatcher;

/// <summary>
/// Configuration options for the file watcher service
/// </summary>
public class FileWatcherOptions
{
    public const string SectionName = "FileWatcher";

    public string WatchDirectory { get; set; } = "C:\\TradeFiles\\Incoming";
    public string FileFilter { get; set; } = "*.csv";
    public int FileProcessingDelayMs { get; set; } = 500;
}
