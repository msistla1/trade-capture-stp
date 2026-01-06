using MediatR;
using Microsoft.Extensions.Logging;
using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Queries;

/// <summary>
/// Handler for GetTradeByIdQuery - implements CQRS Query Handler pattern
/// </summary>
public class GetTradeByIdQueryHandler : IRequestHandler<GetTradeByIdQuery, Result<Trade>>
{
    private readonly ITradeRepository _tradeRepository;
    private readonly ILogger<GetTradeByIdQueryHandler> _logger;

    public GetTradeByIdQueryHandler(
        ITradeRepository tradeRepository,
        ILogger<GetTradeByIdQueryHandler> logger)
    {
        _tradeRepository = tradeRepository;
        _logger = logger;
    }

    public async Task<Result<Trade>> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Querying trade with ID: {TradeId}", request.TradeId);

            var trade = await _tradeRepository.GetByTradeIdAsync(request.TradeId);

            if (trade == null)
            {
                _logger.LogWarning("Trade not found: {TradeId}", request.TradeId);
                return Result.Failure<Trade>($"Trade with ID {request.TradeId} not found");
            }

            _logger.LogInformation("Successfully retrieved trade: {TradeId}", request.TradeId);
            return Result.Success(trade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying trade: {TradeId}", request.TradeId);
            return Result.Failure<Trade>($"Error retrieving trade: {ex.Message}");
        }
    }
}
