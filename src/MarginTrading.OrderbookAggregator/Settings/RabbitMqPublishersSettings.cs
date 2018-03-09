namespace MarginTrading.OrderbookAggregator.Settings
{
    public class RabbitMqPublishersSettings
    {
        public RabbitConnectionSettings Orderbooks { get; set; }
        public RabbitConnectionSettings Started { get; set; }
        public RabbitConnectionSettings Stopping { get; set; }
    }
}
