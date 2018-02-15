using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace MarginTrading.OrderbookAggregator.Contracts.Messages
{
    /// <summary>
    /// Info about best bid and ask for an asset
    /// </summary>
    public class ExternalExchangeOrderbookMessage
    {
        /// <summary>
        /// Source
        /// </summary>
        [JsonProperty("source"), CanBeNull]
        public string ExchangeName { get; set; }

        /// <summary>
        /// Asset pair id
        /// </summary>
        [JsonProperty("asset"), CanBeNull]
        public string AssetPairId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("asks"), ItemCanBeNull]
        public List<VolumePrice> Asks { get; set; }

        [JsonProperty("bids"), ItemCanBeNull]
        public List<VolumePrice> Bids { get; set; }
    }
}