using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.Stac
{
    public class StacConsumerMessage : IKafkaMessage
    {
        [JsonProperty("orderId")]
        public required Guid OrderId { get; set; }
        [JsonProperty("result")]
        public int Result { get; set; }
    }
}
