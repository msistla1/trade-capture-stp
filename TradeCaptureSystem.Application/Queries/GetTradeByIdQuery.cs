using MediatR;
using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Application.Queries;

/// <summary>
/// Query to retrieve a trade by its ID
/// </summary>
public class GetTradeByIdQuery : IRequest<Result<Trade>>
{
    public string TradeId { get; set; }

    public GetTradeByIdQuery(string tradeId)
    {
        TradeId = tradeId;
    }
}
