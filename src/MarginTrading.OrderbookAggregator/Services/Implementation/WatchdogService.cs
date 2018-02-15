using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Infrastructure;
using MarginTrading.OrderbookAggregator.Infrastructure.Implementation;
using MarginTrading.OrderbookAggregator.Models;

namespace MarginTrading.OrderbookAggregator.Services.Implementation
{
    public class WatchdogService : TimerPeriod, IWatchdogService
    {
        private readonly ConcurrentDictionary<(string ExchangeName, string AssetPairId), OutdatedOrderbookInfo>
            _orderbooksUpdateInfos = new ConcurrentDictionary<(string, string), OutdatedOrderbookInfo>();

        private readonly IAlertService _alertService;
        private readonly ISettingsService _settingsService;
        private readonly ISystem _system;
        private readonly IOrderbooksStatusService _orderbooksStatusService;

        public WatchdogService(IAlertService alertService, ISettingsService settingsService, ILog log,
            ISystem system, IOrderbooksStatusService orderbooksStatusService)
            : base(Startup.ServiceName + '_' + nameof(WatchdogService),
                (int) settingsService.GetOutdatingCalculationPeriod().TotalMilliseconds,
                log)
        {
            _alertService = alertService;
            _settingsService = settingsService;
            _system = system;
            _orderbooksStatusService = orderbooksStatusService;
        }

        public void OnOrderbookArrived(string exchangeName, string assetPairId, DateTime time)
        {
            _orderbooksUpdateInfos.AddOrUpdate((exchangeName, assetPairId),
                k => new OutdatedOrderbookInfo {LastUpdateTime = time},
                (k, old) =>
                {
                    old.LastUpdateTime = time;
                    return old;
                });
        }

        public override Task Execute()
        {
            var now = _system.UtcNow;
            var outdatedMessage = new StringBuilder();
            var validMessage = new StringBuilder();
            var minLastAlertTime = now - _settingsService.GetOutdatedAlertRepeatPeriod();
            foreach (var group in _orderbooksUpdateInfos.GroupBy(t => t.Key.ExchangeName))
            {
                var outdatingThreshold = _settingsService.GetOutdatingThreshold(group.Key);
                if (outdatingThreshold == null || outdatingThreshold <= TimeSpan.Zero)
                    continue;

                var minUpdateTime = now - outdatingThreshold;
                var outdatedHeaderAdded = false;
                var validHeaderAdded = false;
                foreach (var pair in group.OrderBy(g => g.Key.AssetPairId))
                {
                    if (pair.Value.LastUpdateTime < minUpdateTime)
                    {
                        if ((pair.Value.LastAlertTime ?? default) < minLastAlertTime)
                        {
                            AddMsgToStringBuilder(outdatedMessage, "Orderbooks from {0} stopped for: ",
                                ref outdatedHeaderAdded, group.Key, pair.Key.AssetPairId, pair.Value.LastUpdateTime);
                            pair.Value.LastAlertTime = now;
                            SetOutdatedStatus(group.Key, pair.Key.AssetPairId);
                        }
                    }
                    else if (pair.Value.LastAlertTime != null)
                    {
                        AddMsgToStringBuilder(validMessage, "Orderbooks from {0} started for: ",
                            ref validHeaderAdded, group.Key, pair.Key.AssetPairId, pair.Value.LastUpdateTime);
                        pair.Value.LastAlertTime = null;
                    }
                }
            }

            if (outdatedMessage.Length > 0)
                _alertService.AlertRiskOfficer("", outdatedMessage.ToString(), EventTypeEnum.OrderbookOutdated);

            if (validMessage.Length > 0)
                _alertService.AlertRiskOfficer("", validMessage.ToString(), EventTypeEnum.OrderbookValid);

            return Task.CompletedTask;
        }

        private void SetOutdatedStatus(string exchangeName, string assetPairId)
        {
            _orderbooksStatusService.UpsertStatus(exchangeName, assetPairId,
                (k, old) => old != null
                    ? new OrderbookStatus(old, OrderbookStatusEnum.Outdated)
                    : OrderbookStatus.Empty);
        }

        private static void AddMsgToStringBuilder(StringBuilder sb, string headerFormat, ref bool headerAdded,
            string exchangeName, string assetPairId, DateTime time)
        {
            var comma = ", ";
            if (!headerAdded)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.AppendFormat(headerFormat, exchangeName);
                headerAdded = true;
                comma = "";
            }

            sb.AppendFormat("{0}{1} (at {2:g})", comma, assetPairId, time);
        }

        private class OutdatedOrderbookInfo
        {
            public DateTime LastUpdateTime { get; set; }
            public DateTime? LastAlertTime { get; set; }
        }
    }
}