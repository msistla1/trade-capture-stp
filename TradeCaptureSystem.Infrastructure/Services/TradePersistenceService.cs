using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Infrastructure.Services;

/// <summary>
/// Simple persistence service that delegates to `ITradeRepository`.
/// </summary>
public class TradePersistenceService : ITradePersistenceService
{
    private readonly ITradeRepository _tradeRepository;

    public TradePersistenceService(ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task SaveAsync(Trade trade)
    {
        await _tradeRepository.SaveAsync(trade);
    }
}
