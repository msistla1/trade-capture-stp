using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;
using TradeCaptureSystem.Domain.Enums;

namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Interface for the trade state machine
/// </summary>
public interface ITradeStateMachine
{
    Task<Result> ProcessTradeAsync(Trade trade);
    Task FireAsync(TcrTrigger trigger);
    TcrState GetCurrentState();
}
