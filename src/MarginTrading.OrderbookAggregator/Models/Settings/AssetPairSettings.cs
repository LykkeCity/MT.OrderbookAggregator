namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class AssetPairSettings
    {
        public AssetPairSettings(AssetPairMarkupsParams markups, string resultingAssetPairId)
        {
            Markups = markups;
            ResultingAssetPairId = resultingAssetPairId;
        }

        public AssetPairMarkupsParams Markups { get; }
        public string ResultingAssetPairId { get; }
    }
}