using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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
    internal class SettingsService : ISettingsService
    {
        private readonly ISettingsRootService _settingsRootService;
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private readonly ISystem _system;
        private readonly ICachedCalculation<Dictionary<string, AssetPairSettings>> _assetPairsSettings;

        public SettingsService(ISettingsRootService settingsRootService, IMtDataReaderClient mtDataReaderClient,
            ISystem system)
        {
            _settingsRootService = settingsRootService;
            _mtDataReaderClient = mtDataReaderClient;
            _system = system;
            _assetPairsSettings = GetAssetPairsSettingsCache();
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
                    return _assetPairsSettings.Get().GetValueOrDefault(basePairId);
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

        private ICachedCalculation<Dictionary<string, AssetPairSettings>> GetAssetPairsSettingsCache()
        {
            return Calculate.Cached(
                () => _system.UtcNow,
                (last, current) => current.Subtract(last) < TimeSpan.FromMinutes(2),
                _ => _mtDataReaderClient.AssetPairSettingsRead.Get(MatchingEngineModeContract.Stp)
                    .GetAwaiter().GetResult().ToDictionary(s => s.BasePairId, CreateAssetPairSettings));
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
    }
}