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
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models.Settings;
using MoreLinq;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class SettingsService : TimerPeriod, ISettingsService, ICustomStartup
    {
        private readonly ISettingsRootService _settingsRootService;
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private readonly ISystem _system;
        private Dictionary<string, AssetPairSettings> _assetPairsSettings;
        private readonly ManualResetEventSlim _assetPairsInitializedEvent = new ManualResetEventSlim();

        public SettingsService(ISettingsRootService settingsRootService, IMtDataReaderClient mtDataReaderClient,
            ISystem system, ILog log) 
            : base(nameof(SettingsService), (int) TimeSpan.FromMinutes(2).TotalMilliseconds, log)
        {
            _settingsRootService = settingsRootService;
            _mtDataReaderClient = mtDataReaderClient;
            _system = system;
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

        private static AssetPairSettings CreateAssetPairSettings(AssetPairSettingsContract s)
        {
            s.AssetPairId.RequiredNotNullOrWhiteSpace(nameof(s.AssetPairId));
            s.BasePairId.RequiredNotNullOrWhiteSpace(nameof(s.BasePairId));
            s.MatchingEngineMode.RequiredEqualsTo(MatchingEngineModeContract.Stp, nameof(s.MatchingEngineMode));
            s.MultiplierMarkupBid.RequiredGreaterThan(0, nameof(s.MultiplierMarkupBid));
            s.MultiplierMarkupAsk.RequiredGreaterThan(0, nameof(s.MultiplierMarkupAsk));

            return new AssetPairSettings(
                new AssetPairMarkupsParams(s.MultiplierMarkupBid, s.MultiplierMarkupAsk),
                s.AssetPairId);
        }

        public override async Task Execute()
        {
            _assetPairsSettings = (await _mtDataReaderClient.AssetPairSettingsRead.Get(MatchingEngineModeContract.Stp))
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