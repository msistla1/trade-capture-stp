using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Domain.Rules;

/// <summary>
/// Validates trade date logic (e.g., not in future, settlement date after trade date)
/// </summary>
public class TradeDateRule : IValidationRule
{
    public Result Validate(Trade trade)
    {
        var errors = new List<string>();

        if (trade.TradeDate > DateTime.UtcNow.AddDays(1))
            errors.Add("Trade date cannot be more than 1 day in the future");

        if (trade.SettlementDate < trade.TradeDate)
            errors.Add("Settlement date must be on or after trade date");

        if (errors.Any())
        {
            foreach (var error in errors)
            {
                trade.AddValidationError(error);
            }
            return Result.Failure(string.Join("; ", errors));
        }

        return Result.Success();
    }
}
