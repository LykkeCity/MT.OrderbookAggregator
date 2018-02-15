using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.OrderbookAggregator.Settings
{
    internal class AppSettings
    {
        public MarginTradingOrderbookAggregatorSettings MarginTradingOrderbookAggregator { get; set; }
        [CanBeNull, Optional]
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public RiskInformingSettings RiskInformingSettings { get; set; }
    }
}
