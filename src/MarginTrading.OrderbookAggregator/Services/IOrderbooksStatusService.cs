using System;
using System.Collections.Generic;
using MarginTrading.OrderbookAggregator.Contracts.Api;
using MarginTrading.OrderbookAggregator.Models;

namespace MarginTrading.OrderbookAggregator.Services
{
    public interface IOrderbooksStatusService
    {
        IReadOnlyList<OrderbookStatusModel> GetAll();
        IReadOnlyList<OrderbookStatusModel> GetByAssetPair(string assetPairId);
        IReadOnlyList<OrderbookStatusModel> GetByExchange(string exchangeName);
        void SetStatus(string exchangeName, string assetPairId, OrderbookStatus info);

        void UpsertStatus(string exchangeName, string assetPairId,
            Func<(string ExchangeName, string AssePairId), OrderbookStatus, OrderbookStatus> update);
    }
}