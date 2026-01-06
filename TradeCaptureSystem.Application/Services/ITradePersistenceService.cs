using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Abstraction for persisting trades
/// </summary>
public interface ITradePersistenceService
{
    Task SaveAsync(Trade trade);
}
