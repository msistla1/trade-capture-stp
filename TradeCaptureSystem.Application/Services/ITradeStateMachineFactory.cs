using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Factory to create configured `ITradeStateMachine` instances per trade
/// </summary>
public interface ITradeStateMachineFactory
{
    ITradeStateMachine Create(Trade trade);
}
