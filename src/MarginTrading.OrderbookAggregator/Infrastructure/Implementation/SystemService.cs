using System;

namespace MarginTrading.OrderbookAggregator.Infrastructure.Implementation
{
    internal class SystemService : ISystem
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
