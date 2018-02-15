using JetBrains.Annotations;

namespace MarginTrading.OrderbookAggregator.AzureRepositories
{
    public interface IAzureBlobJsonStorage
    {
        [CanBeNull] T Read<T>(string container, string key) where T: class;
        void Write<T>(string container, string key, T obj) where T: class;
    }
}