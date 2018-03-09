using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Infrastructure.Implementation
{
    internal static class Trace
    {
        [CanBeNull]
        internal static ITraceService TraceService { get; set; }

        public static void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj)
        {
            TraceService?.Write(levelGroup, assetPairId, msg, obj);
        }
    }
}
