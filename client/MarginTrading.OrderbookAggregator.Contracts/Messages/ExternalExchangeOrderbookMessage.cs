using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MessagePack;
using Newtonsoft.Json;

namespace MarginTrading.OrderbookAggregator.Contracts.Messages
{
    /// <summary>
    /// Info about best bid and ask for an asset
    /// </summary>
    [PublicAPI, MessagePackObject]
    public class ExternalExchangeOrderbookMessage
    {
        /// <summary>
        /// Source
        /// </summary>
        [JsonProperty("source"), Key(0), CanBeNull]
        public string ExchangeName { get; set; }

        /// <summary>
        /// Asset pair id
        /// </summary>
        [JsonProperty("asset"), Key(1), CanBeNull]
        public string AssetPairId { get; set; }

        [JsonProperty("timestamp"), Key(2)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("asks"), Key(3), ItemCanBeNull]
        public List<VolumePrice> Asks { get; set; }

        [JsonProperty("bids"), Key(4), ItemCanBeNull]
        public List<VolumePrice> Bids { get; set; }
    }
}