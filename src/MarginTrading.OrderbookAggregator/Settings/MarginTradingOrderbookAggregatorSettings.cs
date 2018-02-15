using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.OrderbookAggregator.Settings
{
    internal class MarginTradingOrderbookAggregatorSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }

        [CanBeNull, Optional]
        public string ApplicationInsightsKey { get; set; }
    }
}
