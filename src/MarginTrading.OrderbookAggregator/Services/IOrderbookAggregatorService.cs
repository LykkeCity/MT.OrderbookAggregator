using System.Threading.Tasks;
using MarginTrading.OrderbookAggregator.Contracts.Messages;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IOrderbookAggregatorService
    {
        Task ProcessNewExternalOrderbookAsync(ExternalExchangeOrderbookMessage orderbook);
    }
}