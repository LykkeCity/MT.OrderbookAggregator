using System;

namespace MarginTrading.OrderbookAggregator.Contracts.Api
{
    public class OrderbookStatusModel
    {
        public string AssetPairId { get; set; }
        public string ExchangeName { get; set; }
        public decimal BestBid { get; set; }
        public decimal BestAsk { get; set; }
        public decimal ResultingBestBid { get; set; }
        public decimal ResultingBestAsk { get; set; }
        public int BidsDepth { get; set; }
        public int AsksDepth { get; set; }
        public int ResultingsBidsDepth { get; set; }
        public int ResultingsAsksDepth { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string Status { get; set; }
    }
}