using MediatR;
using TradeCaptureSystem.Domain.Common;

namespace TradeCaptureSystem.Application.Commands;

/// <summary>
/// Command to process a new trade through the state machine
/// </summary>
public class ProcessTradeCommand : IRequest<Result>
{
    public string TradeId { get; set; }
    public string Counterparty { get; set; }
    public string Instrument { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime TradeDate { get; set; }
    public DateTime SettlementDate { get; set; }

    public ProcessTradeCommand(string tradeId, string counterparty, string instrument,
                                decimal quantity, decimal price, DateTime tradeDate, DateTime settlementDate)
    {
        TradeId = tradeId;
        Counterparty = counterparty;
        Instrument = instrument;
        Quantity = quantity;
        Price = price;
        TradeDate = tradeDate;
        SettlementDate = settlementDate;
    }
}
