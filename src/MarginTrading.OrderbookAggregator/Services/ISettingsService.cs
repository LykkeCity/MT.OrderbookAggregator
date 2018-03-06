using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services
{
    /// <summary>
    /// Interface for getting non-infrastructural settings (service behaviour settings)
    /// </summary>
    public interface ISettingsService
    {
        [CanBeNull] AssetPairSettings TryGetAssetPair(string exchangeName, string basePairId);
        TimeSpan GetOutdatingCalculationPeriod();
        bool IsTracePersistanceEnabled();
        TimeSpan GetOutdatedAlertRepeatPeriod();
        [CanBeNull] TimeSpan? GetOutdatingThreshold(string exchangeName);
    }
}
