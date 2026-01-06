namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Service for checking duplicate trades
/// </summary>
public interface IDuplicateCheckService
{
    Task<bool> IsDuplicateAsync(string tradeId);
}
