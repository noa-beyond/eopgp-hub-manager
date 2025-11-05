using Domain.Enums;
using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.ChDM
{
    public class ChDetectionMappingConsumerMessage : IKafkaMessage
    {
        [JsonProperty("orderId")]
        public Guid OrderId { get; set; }
        [JsonProperty("result")]
        public OrderResult Result { get; set; }
        [JsonProperty("chdmProductPath")]
        public string? ProductPath { get; set; }
    }
}