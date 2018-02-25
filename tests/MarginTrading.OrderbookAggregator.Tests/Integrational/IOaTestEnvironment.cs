using System;
using System.Collections.Generic;
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational
{
    internal interface IOaTestEnvironment : ITestEnvironment
    {
        DateTime UtcNow { get; set; }
        StubRabbitMqService StubRabbitMqService { get; }
        SettingsRootStorageModel SettingsRoot { get; set; }
        InMemoryTableStorageFactory TableStorageFactory { get; }
        InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; }
        List<AssetPairSettingsContract> AssetPairSettings { get; }
    }
}