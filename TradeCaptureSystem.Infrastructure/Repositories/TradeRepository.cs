using Microsoft.EntityFrameworkCore;
using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Entities;
using TradeCaptureSystem.Infrastructure.Persistence;

namespace TradeCaptureSystem.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Trade aggregate
/// </summary>
public class TradeRepository : ITradeRepository
{
    private readonly TradeCaptureDbContext _context;

    public TradeRepository(TradeCaptureDbContext context)
    {
        _context = context;
    }

    public async Task<Trade?> GetByIdAsync(Guid id)
    {
        return await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trade?> GetByTradeIdAsync(string tradeId)
    {
        return await _context.Trades
            .FirstOrDefaultAsync(t => t.TradeId == tradeId);
    }

    public async Task<IEnumerable<Trade>> GetAllAsync()
    {
        return await _context.Trades.ToListAsync();
    }

    public async Task SaveAsync(Trade trade)
    {
        await _context.Trades.AddAsync(trade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Trade trade)
    {
        _context.Trades.Update(trade);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByTradeIdAsync(string tradeId)
    {
        return await _context.Trades
            .AnyAsync(t => t.TradeId == tradeId);
    }
}
