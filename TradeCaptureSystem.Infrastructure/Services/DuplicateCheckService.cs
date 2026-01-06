using TradeCaptureSystem.Application.Services;

namespace TradeCaptureSystem.Infrastructure.Services;

/// <summary>
/// Service implementation for duplicate checking
/// </summary>
public class DuplicateCheckService : IDuplicateCheckService
{
    private readonly ITradeRepository _tradeRepository;

    public DuplicateCheckService(ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<bool> IsDuplicateAsync(string tradeId)
    {
        return await _tradeRepository.ExistsByTradeIdAsync(tradeId);
    }
}
