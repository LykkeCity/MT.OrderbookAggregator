using System;
using MarginTrading.OrderbookAggregator.Enums;

namespace MarginTrading.OrderbookAggregator.Models
{
    public class OrderbookStatus
    {
        public decimal BestBid { get; }
        public decimal BestAsk { get; }
        public decimal ResultingBestBid { get; }
        public decimal ResultingBestAsk { get; }
        public int BidsDepth { get; }
        public int AsksDepth { get; }
        public int ResultingsBidsDepth { get; }
        public int ResultingsAsksDepth { get; }
        public DateTime? LastUpdateTime { get; }
        public OrderbookStatusEnum Status { get; }

        public OrderbookStatus(decimal bestBid, decimal bestAsk, decimal resultingBestBid, decimal resultingBestAsk,
            int bidsDepth, int asksDepth, int resultingsBidsDepth, int resultingsAsksDepth, DateTime? lastUpdateTime,
            OrderbookStatusEnum status)
        {
            BestBid = bestBid;
            BestAsk = bestAsk;
            ResultingBestBid = resultingBestBid;
            ResultingBestAsk = resultingBestAsk;
            BidsDepth = bidsDepth;
            AsksDepth = asksDepth;
            ResultingsBidsDepth = resultingsBidsDepth;
            ResultingsAsksDepth = resultingsAsksDepth;
            LastUpdateTime = lastUpdateTime;
            Status = status;
        }

        public OrderbookStatus(OrderbookStatus orig, OrderbookStatusEnum status)
            : this(orig.BestBid, orig.BestAsk, orig.ResultingBestBid, orig.ResultingBestAsk,
                orig.BidsDepth, orig.AsksDepth, orig.ResultingsBidsDepth, orig.ResultingsAsksDepth, orig.LastUpdateTime,
                status)
        {
        }

        /// <summary>
        /// Like if no data was ever received
        /// </summary>
        public static OrderbookStatus Empty =>
            new OrderbookStatus(0, 0, 0, 0, 0, 0, 0, 0, null, OrderbookStatusEnum.NoOrderbook);
    }
}