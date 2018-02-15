using System;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IWatchdogService
    {
        void OnOrderbookArrived(string exchangeName, string assetPairId, DateTime time);
    }
}