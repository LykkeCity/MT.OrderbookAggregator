namespace MarginTrading.OrderbookAggregator.Enums
{
    public enum ExchangeModeEnum
    {
        /// <summary>
        ///     No asset pairs are taken
        /// </summary>
        Disabled = 1,
        
        /// <summary>
        ///     Asset pairs which are configured in the asset pairs settings api are taken
        /// </summary>
        TakeConfigured = 2,
    }
}