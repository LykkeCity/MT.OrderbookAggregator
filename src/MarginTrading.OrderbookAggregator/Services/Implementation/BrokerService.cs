using System;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Settings;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class BrokerService : IBrokerService
    {
        private readonly ILog _logger;
        private readonly IOrderbookAggregatorService _orderbookAggregatorService;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IReloadingManager<MarginTradingOrderbookAggregatorSettings> _settings;

        public BrokerService(ILog logger, IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings, IOrderbookAggregatorService orderbookAggregatorService,
            IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _settings = settings;
            _orderbookAggregatorService = orderbookAggregatorService;
            _rabbitMqService = rabbitMqService;
        }

        public void Run()
        {
            _logger.WriteInfoAsync("Broker.Run", null, "Starting broker");
            try
            {
                _rabbitMqService.Subscribe<ExternalExchangeOrderbookMessage>(_settings.Nested(s => s.RabbitMq.Consumers.FiatOrderbooks), true,
                    _orderbookAggregatorService.ProcessNewExternalOrderbookAsync);
                _rabbitMqService.Subscribe<ExternalExchangeOrderbookMessage>(_settings.Nested(s => s.RabbitMq.Consumers.CryptoOrderbooks), true,
                    _orderbookAggregatorService.ProcessNewExternalOrderbookAsync);
            }
            catch (Exception ex)
            {
                _logger.WriteErrorAsync("Broker.Run", null, ex);
            }
        }
    }
}