using System;
using System.Collections.Immutable;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels
{
    internal class ExchangeSettingsStorageModel
    {
        public ExchangeModeEnumStorageModel Mode { get; set; }
        public TimeSpan OutdatingThreshold { get; set; }

        public enum ExchangeModeEnumStorageModel
        {
            Disabled = 1,
            TakeConfigured = 2,
        }
    }
}