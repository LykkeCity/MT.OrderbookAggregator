namespace MarginTrading.OrderbookAggregator.Models.Settings
{
    public class AssetPairMarkupsParams
    {
        public decimal BidMultiplier { get; }
        public decimal AskMultiplier { get; }

        public AssetPairMarkupsParams(decimal bidMultiplier, decimal askMultiplier)
        {
            BidMultiplier = bidMultiplier;
            AskMultiplier = askMultiplier;
        }
    }
}