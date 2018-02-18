using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models;
using MarginTrading.OrderbookAggregator.Settings;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class OrderbookAggregatorService : IOrderbookAggregatorService, IDisposable, ICustomStartup
    {
        private readonly ISettingsService _settingsService;
        private readonly Lazy<IMessageProducer<ExternalExchangeOrderbookMessage>> _messageProducer;
        private readonly ISystem _system;
        private readonly IAlertService _alertService;
        private readonly IBestPricesService _bestPricesService;
        private readonly IWatchdogService _watchdogService;
        private readonly IOrderbooksStatusService _orderbooksStatusService;

        public OrderbookAggregatorService(ISettingsService settingsService,
            IRabbitMqService rabbitMqService,
            ISystem system,
            IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings,
            IAlertService alertService, IBestPricesService bestPricesService, IWatchdogService watchdogService,
            IOrderbooksStatusService orderbooksStatusService)
        {
            _settingsService = settingsService;
            _system = system;
            _alertService = alertService;
            _bestPricesService = bestPricesService;
            _watchdogService = watchdogService;
            _orderbooksStatusService = orderbooksStatusService;
            _messageProducer = new Lazy<IMessageProducer<ExternalExchangeOrderbookMessage>>(() =>
                CreateRabbitMqMessageProducer(settings, rabbitMqService));
        }

        public Task ProcessNewExternalOrderbookAsync(ExternalExchangeOrderbookMessage orderbook)
        {
            var settings = _settingsService.TryGetAssetPair(orderbook.ExchangeName, orderbook.AssetPairId);
            if (settings == null || (orderbook.Bids?.Count ?? 0) == 0 || (orderbook.Asks?.Count ?? 0) == 0)
            {
                return Task.CompletedTask;
            }

            orderbook.Bids.RemoveAll(b => b == null);
            orderbook.Asks.RemoveAll(b => b == null);
            orderbook.Bids.Sort((a, b) => -a.Price.CompareTo(b.Price));
            orderbook.Asks.Sort((a, b) => a.Price.CompareTo(b.Price));

            var now = _system.UtcNow;
            var externalOrderbook = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = orderbook.AssetPairId,
                ExchangeName = orderbook.ExchangeName,
                Timestamp = now,
                Bids = GetOrderbookPositions(orderbook.Bids, settings.Markups.Bid),
                Asks = GetOrderbookPositions(orderbook.Asks, settings.Markups.Ask)
            };

            _watchdogService.OnOrderbookArrived(orderbook.ExchangeName, orderbook.AssetPairId, now);
            WriteStats(orderbook, externalOrderbook, now);
            return _messageProducer.Value.ProduceAsync(externalOrderbook);
        }

        private static List<VolumePrice> GetOrderbookPositions(IEnumerable<VolumePrice> prices,
            decimal markup)
        {
            return prices.GroupBy(p => p.Price).Select(gr => new VolumePrice
                {
                    Price = gr.Key + markup,
                    Volume = gr.Sum(b => b.Volume),
                })
                .ToList();
        }

        private static IMessageProducer<ExternalExchangeOrderbookMessage> CreateRabbitMqMessageProducer(
            IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings, IRabbitMqService rabbitMqService)
        {
            return rabbitMqService.GetProducer(settings.Nested(s => s.RabbitMq.Publishers.Orderbooks), false, 
                rabbitMqService.GetMsgPackSerializer<ExternalExchangeOrderbookMessage>());
        }

        public void Initialize()
        {
            _alertService.AlertStarted();
        }

        public void Dispose()
        {
            _alertService.AlertStopping();
        }
        
        private void WriteStats(ExternalExchangeOrderbookMessage orderbook, 
            ExternalExchangeOrderbookMessage resultingOrderbook, DateTime now)
        {
            var bestPrices = _bestPricesService.Calc(orderbook);
            var resultingBestPrices = _bestPricesService.Calc(resultingOrderbook);
            
            _orderbooksStatusService.SetStatus(orderbook.ExchangeName, orderbook.AssetPairId, 
                new OrderbookStatus(bestPrices.BestBid, bestPrices.BestAsk, 
                    resultingBestPrices.BestBid, resultingBestPrices.BestAsk,
                    orderbook.Bids.Count, orderbook.Asks.Count,
                    resultingOrderbook.Bids.Count, resultingOrderbook.Asks.Count,
                    now, OrderbookStatusEnum.Valid));

            Trace.Write(TraceLevelGroupEnum.Trace, orderbook.AssetPairId,
                $"Orderbook from {orderbook.ExchangeName} for {orderbook.AssetPairId}: {resultingBestPrices.BestBid}/{resultingBestPrices.BestAsk}",
                new
                {
                    Event = "ExternalOrderbookProcessed",
                    Source = orderbook.ExchangeName,
                    bestPrices.BestBid,
                    bestPrices.BestAsk,
                    ResultingBestBid = resultingBestPrices.BestBid,
                    ResultingBestAsk = resultingBestPrices.BestAsk,
                    BidsDepth = orderbook.Bids.Count,
                    AsksDepth = orderbook.Asks.Count,
                    ResultingsBidsDepth = resultingOrderbook.Bids.Count,
                    ResultingsAsksDepth = resultingOrderbook.Asks.Count,
                });
        }
    }
}