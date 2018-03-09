using System.Threading.Tasks;
using Lykke.SettingsReader;
using Lykke.SlackNotifications;
using MarginTrading.OrderbookAggregator.Contracts.Messages;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Settings;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    internal class AlertService : IAlertService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IReloadingManager<MarginTradingOrderbookAggregatorSettings> _settings;
        private readonly ISlackNotificationsSender _slack;
        private readonly IAlertSeverityLevelService _alertSeverityLevelService;

        public AlertService(IRabbitMqService rabbitMqService,
            IReloadingManager<MarginTradingOrderbookAggregatorSettings> settings, ISlackNotificationsSender slack,
            IAlertSeverityLevelService alertSeverityLevelService)
        {
            _rabbitMqService = rabbitMqService;
            _settings = settings;
            _slack = slack;
            _alertSeverityLevelService = alertSeverityLevelService;
        }

        public void AlertRiskOfficer(string assetPairId, string message, EventTypeEnum eventType)
        {
            var (slackChannelType, traceLevel) = _alertSeverityLevelService.GetLevel(eventType);
            Trace.Write(traceLevel, assetPairId, $"{nameof(AlertRiskOfficer)}: {message}", new {});
            if (!string.IsNullOrWhiteSpace(slackChannelType))
                _slack.SendAsync(slackChannelType, "MT OrderbookAggregator", message);
        }

        public void AlertStarted()
        {
            AlertRiskOfficer(null, "Market maker started", EventTypeEnum.StatusInfo);
            _rabbitMqService.GetProducer(_settings.Nested(s => s.RabbitMq.Publishers.Started), true, 
                    _rabbitMqService.GetJsonSerializer<StartedMessage>())
                .ProduceAsync(new StartedMessage());
        }

        public Task AlertStopping()
        {
            AlertRiskOfficer(null, "Market maker stopping", EventTypeEnum.StatusInfo);
            return _rabbitMqService.GetProducer(_settings.Nested(s => s.RabbitMq.Publishers.Stopping), true,
                    _rabbitMqService.GetJsonSerializer<StoppingMessage>())
                .ProduceAsync(new StoppingMessage());
        }
    }
}