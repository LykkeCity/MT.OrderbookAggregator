using Newtonsoft.Json;

namespace MarginTrading.OrderbookAggregator.Contracts.Messages
{
    public class VolumePrice
    {
        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
