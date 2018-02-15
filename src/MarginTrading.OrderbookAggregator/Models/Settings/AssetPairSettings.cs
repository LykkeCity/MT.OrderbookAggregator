namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class AssetPairSettings
    {
        public AssetPairSettings(AssetPairMarkupsParams markups)
        {
            Markups = markups;
        }

        public AssetPairMarkupsParams Markups { get; }
    }
}