using System;
using System.Collections.Immutable;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class ExchangeSettings
    {
        public ExchangeSettings(ExchangeModeEnum mode, ImmutableSortedDictionary<string, AssetPairSettings> assetPairs,
            AssetPairSettings defaultSettings, TimeSpan outdatingThreshold)
        {
            Mode = mode;
            AssetPairs = assetPairs;
            DefaultSettings = defaultSettings;
            OutdatingThreshold = outdatingThreshold;
        }

        public ExchangeModeEnum Mode { get; }
        public ImmutableSortedDictionary<string, AssetPairSettings> AssetPairs { get; }
        public AssetPairSettings DefaultSettings { get; }
        public TimeSpan OutdatingThreshold { get; }
    }
}