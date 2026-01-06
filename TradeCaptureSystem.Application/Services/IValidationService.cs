using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Services;

/// <summary>
/// Service to encapsulate validation logic and rules
/// </summary>
public interface IValidationService
{
    Result Validate(Trade trade);
}
