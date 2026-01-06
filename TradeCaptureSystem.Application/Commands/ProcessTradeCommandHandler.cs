using MediatR;
using Microsoft.Extensions.Logging;
using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Commands;

/// <summary>
/// Handler for ProcessTradeCommand - implements CQRS Command Handler pattern
/// </summary>
public class ProcessTradeCommandHandler : IRequestHandler<ProcessTradeCommand, Result>
{
    private readonly ITradeStateMachineFactory _stateMachineFactory;
    private readonly ILogger<ProcessTradeCommandHandler> _logger;
    private readonly ILoggerFactory _loggerFactory;
    public ProcessTradeCommandHandler(
        ITradeStateMachineFactory stateMachineFactory,
        ILogger<ProcessTradeCommandHandler> logger,
        ILoggerFactory loggerFactory)
    {
        _stateMachineFactory = stateMachineFactory;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<Result> Handle(ProcessTradeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing trade command for TradeId: {TradeId}", request.TradeId);

            // Create trade entity
            var trade = new Trade(
                request.TradeId,
                request.Counterparty,
                request.Instrument,
                request.Quantity,
                request.Price,
                request.TradeDate,
                request.SettlementDate
            );

            // Create and execute state machine via factory
            var stateMachine = _stateMachineFactory.Create(trade);
            var result = await stateMachine.ProcessTradeAsync(trade);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully processed trade {TradeId}", request.TradeId);
            }
            else
            {
                _logger.LogWarning("Failed to process trade {TradeId}: {Error}", request.TradeId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProcessTradeCommand for TradeId: {TradeId}", request.TradeId);
            return Result.Failure($"Error processing trade: {ex.Message}");
        }
    }
}
