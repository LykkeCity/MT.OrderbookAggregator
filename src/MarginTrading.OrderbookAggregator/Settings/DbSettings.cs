using Lykke.SettingsReader.Attributes;

namespace MarginTrading.OrderbookAggregator.Settings
{
    public class DbSettings
    {
        [AzureBlobCheck]
        public string ConnectionString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureBlobCheck]
        public string QueuePersistanceRepositoryConnString { get; set; }
    }
}
