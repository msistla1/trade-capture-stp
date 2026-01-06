using Microsoft.Extensions.Logging;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Services;

public class TradeStateMachineFactory : ITradeStateMachineFactory
{
    private readonly IValidationService _validationService;
    private readonly IDuplicateCheckService _duplicateCheckService;
    private readonly ITradePersistenceService _tradePersistenceService;
    private readonly ILoggerFactory _loggerFactory;

    public TradeStateMachineFactory(
        IValidationService validationService,
        IDuplicateCheckService duplicateCheckService,
        ITradePersistenceService tradePersistenceService,
        ILoggerFactory loggerFactory)
    {
        _validationService = validationService;
        _duplicateCheckService = duplicateCheckService;
        _tradePersistenceService = tradePersistenceService;
        _loggerFactory = loggerFactory;
    }

    public ITradeStateMachine Create(Trade trade)
    {
        return new TradeStateMachine(
            trade,
            _validationService,
            _duplicateCheckService,
            _tradePersistenceService,
            _loggerFactory.CreateLogger<TradeStateMachine>());
    }
}
