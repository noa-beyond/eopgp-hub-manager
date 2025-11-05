using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.Gateway
{
    public class GatewayMessage : IKafkaMessage
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }
    }
}