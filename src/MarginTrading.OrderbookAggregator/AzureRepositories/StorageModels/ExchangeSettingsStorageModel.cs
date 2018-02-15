using System;
using System.Collections.Immutable;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels
{
    internal class ExchangeSettingsStorageModel
    {
        public AssetPairSettingsStorageModel DefaultSettings { get; set; }
        public ExchangeModeEnum Mode { get; set; }
        public ImmutableSortedDictionary<string, AssetPairSettingsStorageModel> AssetPairs { get; set; }
            = ImmutableSortedDictionary<string, AssetPairSettingsStorageModel>.Empty;
        public TimeSpan OutdatingThreshold { get; set; }
    }
}