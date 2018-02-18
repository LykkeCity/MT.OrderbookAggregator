using System;
using System.Threading.Tasks;
using Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.SettingsReader;
using MarginTrading.OrderbookAggregator.Settings;

namespace MarginTrading.OrderbookAggregator.Infrastructure
{
    public interface IRabbitMqService
    {
        IMessageProducer<TMessage> GetProducer<TMessage>(IReloadingManager<RabbitConnectionSettings> settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer);

        void Subscribe<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable,
            Func<TMessage, Task> handler);

        IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>();
        IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>();
    }
}