using System;
using System.Collections.Immutable;

namespace MarginTrading.OrderbookAggregator.AzureRepositories.StorageModels
{
    internal class SettingsRootStorageModel
    {
        public int Version { get; set; }
        
        public ImmutableSortedDictionary<string, ExchangeSettingsStorageModel> Exchanges { get; set; }
            = ImmutableSortedDictionary<string, ExchangeSettingsStorageModel>.Empty;
        public TimeSpan OutdationCheckPeriod { get; set; }
        public TimeSpan OutdationAlertRepeatPeriod { get; set; }
        public bool PersistTrace { get; set; }
    }
}