using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class SettingsRoot
    {
        public ImmutableSortedDictionary<string, ExchangeSettings> Exchanges { get; }
        public TimeSpan OutdationCheckPeriod { get; }
        public TimeSpan OutdationAlertRepeatPeriod { get; }
        public bool PersistTrace { get; }

        public SettingsRoot([NotNull] ImmutableSortedDictionary<string, ExchangeSettings> exchanges,
            TimeSpan outdationCheckPeriod, TimeSpan outdationAlertRepeatPeriod, bool persistTrace)
        {
            Exchanges = exchanges ?? throw new ArgumentNullException(nameof(exchanges));
            OutdationCheckPeriod = outdationCheckPeriod;
            OutdationAlertRepeatPeriod = outdationAlertRepeatPeriod;
            PersistTrace = persistTrace;
        }
    }
}