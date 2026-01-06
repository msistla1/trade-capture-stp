using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Repository interface for Trade aggregate following repository pattern
/// </summary>
public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(Guid id);
    Task<Trade?> GetByTradeIdAsync(string tradeId);
    Task<IEnumerable<Trade>> GetAllAsync();
    Task SaveAsync(Trade trade);
    Task UpdateAsync(Trade trade);
    Task<bool> ExistsByTradeIdAsync(string tradeId);
}
