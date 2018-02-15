using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MarginTrading.OrderbookAggregator.Contracts.Api;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Models;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    public class OrderbooksStatusService : IOrderbooksStatusService
    {
        private readonly ConcurrentDictionary<(string ExchangeName, string AssePairId), OrderbookStatus>
            _orderbooksUpdateInfos = new ConcurrentDictionary<(string, string), OrderbookStatus>();
        
        private readonly IConvertService _convertService;

        public OrderbooksStatusService(IConvertService convertService)
        {
            _convertService = convertService;
        }

        public void SetStatus(string exchangeName, string assetPairId, OrderbookStatus info)
        {
            _orderbooksUpdateInfos[(exchangeName, assetPairId)] = info;
            // todo: log changes?
        }

        public void UpsertStatus(string exchangeName, string assetPairId,
            Func<(string ExchangeName, string AssePairId), OrderbookStatus, OrderbookStatus> update)
        {
            _orderbooksUpdateInfos.AddOrUpdate((exchangeName, assetPairId), k => update(k, null), update);
            // todo: log changes?
        }

        public IReadOnlyList<OrderbookStatusModel> GetAll()
        {
            return _orderbooksUpdateInfos.ToArray()
                .Select(p => Convert(p.Key.ExchangeName, p.Key.AssePairId, p.Value))
                .OrderBy(q => q.AssetPairId)
                .ThenBy(q => q.ExchangeName)
                .ToList();
        }

        public IReadOnlyList<OrderbookStatusModel> GetByAssetPair(string assetPairId)
        {
            return GetAll().Where(m => m.AssetPairId == assetPairId).ToList();
        }

        public IReadOnlyList<OrderbookStatusModel> GetByExchange(string exchangeName)
        {
            return GetAll().Where(m => m.ExchangeName == exchangeName).ToList();
        }

        private OrderbookStatusModel Convert(string exchangeName, string assetPairId, OrderbookStatus orderbookStatus)
        {
            var model = _convertService.Convert<OrderbookStatus, OrderbookStatusModel>(orderbookStatus,
                o => o.ConfigureMap(MemberList.Source));
            model.AssetPairId = assetPairId;
            model.ExchangeName = exchangeName;
            return model;
        }
    }
}