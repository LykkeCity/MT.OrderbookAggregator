﻿using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface ISettingsService
    {
        [CanBeNull] AssetPairSettings TryGetAssetPair(string exchangeName, string assetPairId);
        TimeSpan GetOutdatingCalculationPeriod();
        bool IsTracePersistanceEnabled();
        TimeSpan GetOutdatedAlertRepeatPeriod();
        [CanBeNull] TimeSpan? GetOutdatingThreshold(string exchangeName);
    }
}
