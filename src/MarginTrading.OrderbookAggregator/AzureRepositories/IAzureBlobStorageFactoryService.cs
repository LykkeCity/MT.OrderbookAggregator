using Lykke.SettingsReader;

namespace MarginTrading.OrderbookAggregator.AzureRepositories
{
    public interface IAzureBlobStorageFactoryService
    {
        IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager);
    }
}