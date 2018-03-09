using System;

namespace MarginTrading.OrderbookAggregator.Infrastructure
{
    public interface ISystem
    {
        DateTime UtcNow { get; }
    }
}