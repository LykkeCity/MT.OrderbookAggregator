using System.Linq;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Models;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    public class BestPricesService : IBestPricesService
    {
        [Pure]
        public BestPrices Calc(ExternalExchangeOrderbookMessage orderbook)
        {
            // note: assume prices sorted best first
            return new BestPrices(orderbook.Bids.First().Price, orderbook.Asks.First().Price);
        }
    }
}
