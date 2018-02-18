using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Logs;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SlackNotifications;
using MarginTrading.OrderbookAggregator.AzureRepositories;
using MarginTrading.OrderbookAggregator.AzureRepositories.Implementation;
using MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Modules;
using MarginTrading.OrderbookAggregator.Settings;
using Moq;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational
{
    internal class OaIntegrationalTestSuit : IntegrationalTestSuit
    {
        private static readonly LogToConsole LogToConsole = new LogToConsole();

        public AppSettings AppSettings { get; set; } = new AppSettings
        {
            MarginTradingOrderbookAggregator = new MarginTradingOrderbookAggregatorSettings
            {
                Db = new DbSettings {QueuePersistanceRepositoryConnString = "fake"}
            },
            RiskInformingSettings = new RiskInformingSettings {Data = new RiskInformingParams[0]}
        };

        public OaIntegrationalTestSuit()
        {
            WithModule(new OrderbookAggregatorModule(
                new StubReloadingManager<AppSettings>(() => AppSettings), LogToConsole));
        }

        public new IOaTestEnvironment Build()
        {
            return (IOaTestEnvironment) base.Build();
        }

        protected override TestEnvironment GetTestContainerBuilder()
        {
            return new OaTestEnvironment(this);
        }

        private class OaTestEnvironment : TestEnvironment, IOaTestEnvironment
        {
            public DateTime UtcNow { get; set; } = DateTime.UtcNow;
            public StubRabbitMqService StubRabbitMqService { get; } = new StubRabbitMqService();
            public InMemoryTableStorageFactory TableStorageFactory { get; } = new InMemoryTableStorageFactory();
            public InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; } =
                new InMemoryBlobStorageSingleObjectFactory();

            public SettingsRootStorageModel SettingsRoot
            {
                get => BlobStorageFactory.Blob.GetObject<SettingsRootStorageModel>();
                set => BlobStorageFactory.Blob.Object = value;
            }

            public OaTestEnvironment(OaIntegrationalTestSuit suit) : base(suit)
            {
                Setup<IRabbitMqService>(StubRabbitMqService)
                    .Setup<ISystem>(m => m.Setup(s => s.UtcNow).Returns(() => UtcNow))
                    .Setup<ILog>(LogToConsole)
                    .Setup<ICandleshistoryservice>()
                    .Setup(new LykkeLogToAzureStorage(null))
                    .Setup<ISlackNotificationsSender>(s =>
                        s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) == Task.CompletedTask)
                    .Setup<IAzureTableStorageFactoryService>(TableStorageFactory)
                    .Setup<IAzureBlobStorageFactoryService>(BlobStorageFactory);
                SettingsRoot = GetDefaultSettingsRoot();
            }

            public override IContainer CreateContainer()
            {
                var container = base.CreateContainer();
                Trace.TraceService = container.Resolve<ITraceService>();
                return container;
            }

            private static SettingsRootStorageModel GetDefaultSettingsRoot()
            {
                return new SettingsRootStorageModel
                {
                    Exchanges = ImmutableSortedDictionary<string, ExchangeSettingsStorageModel>.Empty.Add("bitmex",
                        new ExchangeSettingsStorageModel
                        {
                            AssetPairs = ImmutableSortedDictionary<string, AssetPairSettingsStorageModel>.Empty,
                            DefaultSettings = new AssetPairSettingsStorageModel
                            {
                                Markups = new MarkupSettingsStorageModel
                                {
                                    Bid = 0, 
                                    Ask = 0,
                                }
                            },
                            Mode = ExchangeModeEnum.TakeAll,
                            OutdatingThreshold = TimeSpan.FromSeconds(10),
                        }),
                    OutdationCheckPeriod = TimeSpan.FromSeconds(10),
                    OutdationAlertRepeatPeriod = TimeSpan.FromSeconds(20),
                    PersistTrace = false,
                    Version = SettingsStorageService.CurrentStorageModelVersion,
                };
            }
        }
    }
}