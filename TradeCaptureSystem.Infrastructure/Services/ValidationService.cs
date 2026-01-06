using Microsoft.Extensions.Logging;
using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;
using TradeCaptureSystem.Domain.Rules;

namespace TradeCaptureSystem.Infrastructure.Services;

/// <summary>
/// Implementation of validation orchestration that runs registered rules
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IEnumerable<IValidationRule> _validationRules;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(IEnumerable<IValidationRule> validationRules, ILogger<ValidationService> logger)
    {
        _validationRules = validationRules;
        _logger = logger;
    }

    public Result Validate(Trade trade)
    {
        _logger.LogInformation("Running validation rules for trade {TradeId}", trade.TradeId);

        foreach (var rule in _validationRules)
        {
            var result = rule.Validate(trade);
            if (result.IsFailure)
            {
                _logger.LogWarning("Validation rule {Rule} failed for trade {TradeId}: {Error}", rule.GetType().Name, trade.TradeId, result.Error);
                return result;
            }
        }

        _logger.LogInformation("All validation rules passed for trade {TradeId}", trade.TradeId);
        return Result.Success();
    }
}
