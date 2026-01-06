using TradeCaptureSystem.Domain.Enums;

namespace TradeCaptureSystem.Domain.Entities;

/// <summary>
/// Represents a Trade aggregate root in the domain
/// </summary>
public class Trade
{
    public Guid Id { get; private set; }
    public string TradeId { get; private set; }
    public string Counterparty { get; private set; }
    public string Instrument { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime TradeDate { get; private set; }
    public DateTime SettlementDate { get; private set; }
    public TcrState CurrentState { get; private set; }
    public bool IsDuplicate { get; private set; }
    public bool IsRetryable { get; private set; }
    public List<string> ValidationErrors { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // For EF Core
    private Trade()
    {
        TradeId = string.Empty;
        Counterparty = string.Empty;
        Instrument = string.Empty;
        ValidationErrors = new List<string>();
    }

    public Trade(string tradeId, string counterparty, string instrument,
                 decimal quantity, decimal price, DateTime tradeDate, DateTime settlementDate)
    {
        Id = Guid.NewGuid();
        TradeId = tradeId;
        Counterparty = counterparty;
        Instrument = instrument;
        Quantity = quantity;
        Price = price;
        TradeDate = tradeDate;
        SettlementDate = settlementDate;
        CurrentState = TcrState.Received;
        IsRetryable = true;
        ValidationErrors = new List<string>();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateState(TcrState newState)
    {
        CurrentState = newState;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDuplicate()
    {
        IsDuplicate = true;
    }

    public void AddValidationError(string error)
    {
        ValidationErrors.Add(error);
    }

    public void MarkAsNonRetryable()
    {
        IsRetryable = false;
    }

    public bool HasRequiredFields()
    {
        return !string.IsNullOrWhiteSpace(TradeId) &&
               !string.IsNullOrWhiteSpace(Counterparty) &&
               !string.IsNullOrWhiteSpace(Instrument) &&
               Quantity > 0 &&
               Price > 0;
    }

    public void UpdateTradeDetails(decimal quantity, decimal price, DateTime settlementDate)
    {
        Quantity = quantity;
        Price = price;
        SettlementDate = settlementDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
