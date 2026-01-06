using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Domain.Rules;

/// <summary>
/// Validates that all required fields are present
/// </summary>
public class RequiredFieldsRule : IValidationRule
{
    public Result Validate(Trade trade)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(trade.TradeId))
            errors.Add("TradeId is required");

        if (string.IsNullOrWhiteSpace(trade.Counterparty))
            errors.Add("Counterparty is required");

        if (string.IsNullOrWhiteSpace(trade.Instrument))
            errors.Add("Instrument is required");

        if (trade.Quantity <= 0)
            errors.Add("Quantity must be greater than zero");

        if (trade.Price <= 0)
            errors.Add("Price must be greater than zero");

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
