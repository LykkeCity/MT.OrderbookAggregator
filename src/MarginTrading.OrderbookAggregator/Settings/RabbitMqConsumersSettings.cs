namespace MarginTrading.OrderbookAggregator.Settings
{
    public class RabbitMqConsumersSettings
    {
        public RabbitConnectionSettings FiatOrderbooks { get; set; }
        public RabbitConnectionSettings CryptoOrderbooks { get; set; }
    }
}
