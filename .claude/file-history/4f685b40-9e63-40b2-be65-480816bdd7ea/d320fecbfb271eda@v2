using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeCaptureSystem.Application.Commands;

namespace TradeCaptureSystem.Infrastructure.FileWatcher;

/// <summary>
/// Background service that watches for trade files and processes them
/// </summary>
public class TradeFileWatcherService : BackgroundService
{
    private readonly ILogger<TradeFileWatcherService> _logger;
    private readonly IMediator _mediator;
    private readonly FileWatcherOptions _options;
    private FileSystemWatcher? _fileWatcher;

    public TradeFileWatcherService(
        ILogger<TradeFileWatcherService> logger,
        IMediator mediator,
        IOptions<FileWatcherOptions> options)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trade File Watcher Service starting. Watching directory: {Directory}",
            _options.WatchDirectory);

        if (!Directory.Exists(_options.WatchDirectory))
        {
            _logger.LogWarning("Watch directory does not exist. Creating: {Directory}", _options.WatchDirectory);
            Directory.CreateDirectory(_options.WatchDirectory);
        }

        _fileWatcher = new FileSystemWatcher(_options.WatchDirectory)
        {
            Filter = _options.FileFilter,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _fileWatcher.Created += OnFileCreated;
        _fileWatcher.Error += OnError;

        _logger.LogInformation("Trade File Watcher Service started successfully");

        return Task.CompletedTask;
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("New trade file detected: {FileName}", e.Name);

        try
        {
            // Wait a bit to ensure file is fully written
            await Task.Delay(_options.FileProcessingDelayMs);

            // Parse and process the file
            await ProcessTradeFileAsync(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trade file: {FileName}", e.Name);
        }
    }

    private async Task ProcessTradeFileAsync(string filePath)
    {
        _logger.LogInformation("Processing trade file: {FilePath}", filePath);

        try
        {
            // Read file content
            var lines = await File.ReadAllLinesAsync(filePath);

            // Skip header if exists
            var dataLines = lines.Skip(1);

            foreach (var line in dataLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var trade = ParseTradeLine(line);

                if (trade != null)
                {
                    // Send command through MediatR for processing
                    var result = await _mediator.Send(trade);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Successfully processed trade: {TradeId}", trade.TradeId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to process trade: {TradeId}, Error: {Error}",
                            trade.TradeId, result.Error);
                    }
                }
            }

            // Move processed file to archive
            MoveToArchive(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading trade file: {FilePath}", filePath);
            MoveToError(filePath);
        }
    }

    private ProcessTradeCommand? ParseTradeLine(string line)
    {
        try
        {
            // Assuming CSV format: TradeId,Counterparty,Instrument,Quantity,Price,TradeDate,SettlementDate
            var fields = line.Split(',');

            if (fields.Length < 7)
            {
                _logger.LogWarning("Invalid trade line format: {Line}", line);
                return null;
            }

            return new ProcessTradeCommand(
                tradeId: fields[0].Trim(),
                counterparty: fields[1].Trim(),
                instrument: fields[2].Trim(),
                quantity: decimal.Parse(fields[3].Trim()),
                price: decimal.Parse(fields[4].Trim()),
                tradeDate: DateTime.Parse(fields[5].Trim()),
                settlementDate: DateTime.Parse(fields[6].Trim())
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing trade line: {Line}", line);
            return null;
        }
    }

    private void MoveToArchive(string filePath)
    {
        try
        {
            var archiveDir = Path.Combine(_options.WatchDirectory, "Archive");
            Directory.CreateDirectory(archiveDir);

            var fileName = Path.GetFileName(filePath);
            var archivePath = Path.Combine(archiveDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}");

            File.Move(filePath, archivePath);
            _logger.LogInformation("Moved processed file to archive: {ArchivePath}", archivePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file to archive: {FilePath}", filePath);
        }
    }

    private void MoveToError(string filePath)
    {
        try
        {
            var errorDir = Path.Combine(_options.WatchDirectory, "Error");
            Directory.CreateDirectory(errorDir);

            var fileName = Path.GetFileName(filePath);
            var errorPath = Path.Combine(errorDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}");

            File.Move(filePath, errorPath);
            _logger.LogWarning("Moved failed file to error directory: {ErrorPath}", errorPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file to error directory: {FilePath}", filePath);
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "File watcher error occurred");
    }

    public override void Dispose()
    {
        _fileWatcher?.Dispose();
        base.Dispose();
    }
}
