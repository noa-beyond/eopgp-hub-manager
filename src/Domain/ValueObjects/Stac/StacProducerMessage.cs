using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.Stac
{
    public class StacProducerMessage : IKafkaMessage
    {
        [JsonProperty("orderId")]
        public Guid OrderId { get; set; }
        [JsonProperty("noaS3Path")]
        public required List<string> ProductPaths { get; set; }
    }
}