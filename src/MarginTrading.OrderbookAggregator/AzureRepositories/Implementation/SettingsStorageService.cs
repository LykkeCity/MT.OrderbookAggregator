using System.Text;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Models.Settings;
using MarginTrading.OrderbookAggregator.Settings;
using Newtonsoft.Json;

namespace MarginTrading.OrderbookAggregator.AzureRepositories.Implementation
{
    internal class SettingsStorageService : ISettingsStorageService
    {
        private readonly IConvertService _convertService;
        private readonly IAzureBlobJsonStorage _blobStorage;
        private const string BlobContainer = "mtoasettings";
        private const string Key = "SettingsRoot";
        public const int CurrentStorageModelVersion = 1;

        public SettingsStorageService(IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings,
            IConvertService convertService, IAzureBlobStorageFactoryService azureBlobStorageFactoryService)
        {
            _convertService = convertService;
            _blobStorage = azureBlobStorageFactoryService.Create(settings.Nested(s => s.Db.ConnectionString));
        }

        public SettingsRoot Read()
        {
            
            var settingsRootStorageModel = _blobStorage.Read<SettingsRootStorageModel>(BlobContainer, Key);
            if (settingsRootStorageModel == null)
                return null;

            return Convert(settingsRootStorageModel);
        }

        private SettingsRoot Convert(SettingsRootStorageModel model)
        {
            return _convertService.Convert<SettingsRootStorageModel, SettingsRoot>(model);
        }

        private SettingsRootStorageModel Convert(SettingsRoot root)
        {
            return _convertService.Convert<SettingsRoot, SettingsRootStorageModel>(root,
                o => o.ConfigureMap().ForMember(m => m.Version, c => c.Ignore()));
        }

        public void Write(SettingsRoot model)
        {
            var storageModel = Convert(model);
            storageModel.Version = CurrentStorageModelVersion;
            _blobStorage.Write(BlobContainer, Key, storageModel);
        }
    }
}