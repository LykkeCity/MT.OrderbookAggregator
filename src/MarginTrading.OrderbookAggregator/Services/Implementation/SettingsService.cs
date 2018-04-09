using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models.Settings;
using MarginTrading.OrderbookAggregator.Settings;
using MoreLinq;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class SettingsService : TimerPeriod, ISettingsService, ICustomStartup
    {
        private readonly ISettingsRootService _settingsRootService;
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private Dictionary<string, AssetPairSettings> _assetPairsSettings;
        private readonly IReloadingManager<MarginTradingOrderbookAggregatorSettings> _settings;

        private readonly ManualResetEventSlim _assetPairsInitializedEvent = new ManualResetEventSlim();

        public SettingsService(ISettingsRootService settingsRootService, IMtDataReaderClient mtDataReaderClient,
            ILog log, IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings)
            : base(nameof(SettingsService), (int) TimeSpan.FromMinutes(3).TotalMilliseconds + 1, log)
        {
            _settingsRootService = settingsRootService;
            _mtDataReaderClient = mtDataReaderClient;
            _settings = settings;
        }

        public AssetPairSettings TryGetAssetPair(string exchangeName, string basePairId)
        {
            var exchange = TryGetExchange(exchangeName);
            if (exchange == null)
                return null;

            switch (exchange.Mode)
            {
                case ExchangeModeEnum.Disabled:
                    return null;
                case ExchangeModeEnum.TakeConfigured:
                    return _assetPairsSettings.GetValueOrDefault(basePairId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(exchange.Mode), exchange.Mode, string.Empty);
            }
        }

        public TimeSpan GetOutdatingCalculationPeriod()
        {
            return _settingsRootService.Get().OutdationCheckPeriod;
        }

        [CanBeNull]
        private ExchangeSettings TryGetExchange(string exchangeName)
        {
            return _settingsRootService.Get().Exchanges.GetValueOrDefault(exchangeName);
        }

        public bool IsTracePersistanceEnabled()
        {
            return _settingsRootService.Get().PersistTrace;
        }

        public TimeSpan GetOutdatedAlertRepeatPeriod()
        {
            return _settingsRootService.Get().OutdationAlertRepeatPeriod;
        }

        public TimeSpan? GetOutdatingThreshold(string exchangeName)
        {
            return TryGetExchange(exchangeName)?.OutdatingThreshold;
        }

        private static AssetPairSettings CreateAssetPairSettings(AssetPairContract s)
        {
            s.Id.RequiredNotNullOrWhiteSpace(nameof(s.Id));
            s.BasePairId.RequiredNotNullOrWhiteSpace(nameof(s.BasePairId));
            s.MatchingEngineMode.RequiredEqualsTo(MatchingEngineModeContract.Stp, nameof(s.MatchingEngineMode));
            s.StpMultiplierMarkupBid.RequiredGreaterThan(0, nameof(s.StpMultiplierMarkupBid));
            s.StpMultiplierMarkupAsk.RequiredGreaterThan(0, nameof(s.StpMultiplierMarkupAsk));

            return new AssetPairSettings(
                new AssetPairMarkupsParams(s.StpMultiplierMarkupBid, s.StpMultiplierMarkupAsk),
                s.Id);
        }

        public override async Task Execute()
        {
            _assetPairsSettings = (await _mtDataReaderClient.AssetPairsRead.List(_settings.CurrentValue.LegalEntity,
                    MatchingEngineModeContract.Stp))
                .ToDictionary(s => s.BasePairId, CreateAssetPairSettings);
            _assetPairsInitializedEvent.Set();
        }

        public void Initialize()
        {
            Start();
            _assetPairsInitializedEvent.Wait();
        }
    }
}