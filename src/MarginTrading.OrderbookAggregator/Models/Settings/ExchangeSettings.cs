using System;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class ExchangeSettings
    {
        public ExchangeSettings(ExchangeModeEnum mode, TimeSpan outdatingThreshold)
        {
            Mode = mode;
            OutdatingThreshold = outdatingThreshold;
        }

        public ExchangeModeEnum Mode { get; }
        public TimeSpan OutdatingThreshold { get; }
    }
}