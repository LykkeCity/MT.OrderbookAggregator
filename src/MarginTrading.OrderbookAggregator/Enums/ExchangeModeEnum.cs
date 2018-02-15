using MarginTrading.OrderbookAggregator.Models.Settings;

namespace MarginTrading.OrderbookAggregator.Enums
{
    public enum ExchangeModeEnum
    {
        /// <summary>
        ///     No asset pairs are taken
        /// </summary>
        Disabled = 1,
        
        /// <summary>
        ///     Asset pairs, added to <see cref="ExchangeSettings.AssetPairs"/> are taken with their configs,
        ///     and all others - using <see cref="ExchangeSettings.DefaultSettings"/> config.
        /// </summary>
        TakeAll = 2,
        
        /// <summary>
        ///     Only asset pairs explicitly added to <see cref="ExchangeSettings.AssetPairs"/> are taken
        /// </summary>
        UseOnlyExplicitlyConfigured = 3,
    }
}