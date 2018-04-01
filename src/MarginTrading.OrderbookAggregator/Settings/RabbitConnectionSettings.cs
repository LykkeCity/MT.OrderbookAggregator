﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.OrderbookAggregator.Settings
{
    public class RabbitConnectionSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
        [Optional, CanBeNull]
        public string AdditionalQueueSuffix { get; set; }
    }
}
