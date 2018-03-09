using Lykke.SettingsReader;

namespace MarginTrading.OrderbookAggregator.AzureRepositories.Implementation
{
    public class AzureBlobStorageFactoryService : IAzureBlobStorageFactoryService
    {
        public IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager)
        {
            return new AzureBlobJsonStorage(connectionStringManager);
        }
    }
}