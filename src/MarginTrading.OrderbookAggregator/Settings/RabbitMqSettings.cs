namespace MarginTrading.OrderbookAggregator.Settings
{
    public class RabbitMqSettings
    {
        public RabbitMqPublishersSettings Publishers { get; set; }
        public RabbitMqConsumersSettings Consumers { get; set; }
    }
}