using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Lykke.Logs;
using Lykke.SlackNotifications;
using MarginTrading.OrderbookAggregator.Enums;
using MarginTrading.OrderbookAggregator.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.OrderbookAggregator.Infrastructure.Implementation
{
    internal class TraceService : ITraceService, ICustomStartup
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new StringEnumConverter()}
        };
        private long _counter;
        
        private readonly BlockingCollection<TraceMessage> WritingQueue =
            new BlockingCollection<TraceMessage>(50000);

        private readonly ISystem _system;
        private readonly LykkeLogToAzureStorage _logToAzureStorage;
        private readonly ISlackNotificationsSender _slackNotificationsSender;
        private readonly ISettingsService _settingsService;

        public TraceService(ISystem system, LykkeLogToAzureStorage logToAzureStorage,
            ISlackNotificationsSender slackNotificationsSender, ISettingsService settingsService)
        {
            _system = system;
            _logToAzureStorage = logToAzureStorage;
            _slackNotificationsSender = slackNotificationsSender;
            _settingsService = settingsService;
        }

        public void Initialize()
        {
            Task.Run(() =>
            {
                while (true)
                    try
                    {
                        foreach (var m in WritingQueue.GetConsumingEnumerable())
                        {
                            var message = m.AssetPairId + '\t' + m.TraceGroup + '\t' + m.Msg;
                            Console.WriteLine(message);
                            if (_settingsService.IsTracePersistanceEnabled())
                            {
                                _logToAzureStorage.WriteInfoAsync("MtOaTrace",
                                    JsonConvert.SerializeObject(m, JsonSerializerSettings), message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _slackNotificationsSender.SendErrorAsync(e.ToAsyncString());
                    }
            });
        }

        public void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj)
        {
            var id = Interlocked.Increment(ref _counter);
            if (!WritingQueue.TryAdd(new TraceMessage(id, levelGroup, assetPairId, msg, obj, _system.UtcNow)))
                Console.WriteLine("ERROR WRITING TO TRACE QUEUE:\t" + assetPairId + '\t' + levelGroup + '\t' + msg);
        }

        public class TraceMessage
        {
            public long Id { get; }
            public TraceLevelGroupEnum TraceGroup { get; }
            public string AssetPairId { get; }
            public string Msg { get; }
            public object Data { get; }
            public DateTime Time { get; }

            public TraceMessage(long id, TraceLevelGroupEnum traceGroup, string assetPairId, string msg, object data, DateTime time)
            {
                TraceGroup = traceGroup;
                AssetPairId = assetPairId;
                Msg = msg;
                Data = data;
                Time = time;
                Id = id;
            }
        }
    }
}