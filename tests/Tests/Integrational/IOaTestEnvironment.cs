﻿using System;
using MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels;

namespace Tests.Integrational
{
    internal interface IOaTestEnvironment : ITestEnvironment
    {
        DateTime UtcNow { get; set; }
        StubRabbitMqService StubRabbitMqService { get; }
        SettingsRootStorageModel SettingsRoot { get; set; }
        InMemoryTableStorageFactory TableStorageFactory { get; }
        InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; }
    }
}