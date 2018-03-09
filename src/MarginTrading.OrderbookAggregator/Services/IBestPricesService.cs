using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Models;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IBestPricesService
    {
        [Pure]
        BestPrices Calc(ExternalExchangeOrderbookMessage orderbook);
    }
}