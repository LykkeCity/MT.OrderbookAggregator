namespace MarginTrading.OrderbookAggregator.Settings
{
    internal class MarginTradingOrderbookAggregatorSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public string LegalEntity { get; set; }
    }
}
