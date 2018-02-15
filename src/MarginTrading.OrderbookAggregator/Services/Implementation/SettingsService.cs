using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models.Settings;
using MoreLinq;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class SettingsService : ISettingsService
    {
        private readonly ISettingsRootService _settingsRootService;

        public SettingsService(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
        }

        public AssetPairSettings TryGetAssetPair(string exchangeName, string assetPairId)
        {
            var exchange = TryGetExchange(exchangeName);
            if (exchange == null)
                return null;
            
            switch (exchange.Mode)
            {
                case ExchangeModeEnum.Disabled:
                    return null;
                case ExchangeModeEnum.TakeAll:
                    return exchange.AssetPairs.GetValueOrDefault(assetPairId)
                           ?? exchange.DefaultSettings;
                case ExchangeModeEnum.UseOnlyExplicitlyConfigured:
                    return exchange.AssetPairs.GetValueOrDefault(assetPairId);
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
    }
}