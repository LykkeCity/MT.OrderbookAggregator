using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Infrastructure
{
    public interface ITraceService
    {
        void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj);
    }
}