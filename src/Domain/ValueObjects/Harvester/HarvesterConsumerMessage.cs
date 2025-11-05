using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.Harvester
{
    public class HarvesterConsumerMessage : IKafkaMessage
    {
        [JsonProperty("succeeded")]
        public List<Guid?> Succeded { get; set; } = [];

        [JsonProperty("failed")]
        public List<Guid?> Failed { get; set; } = [];
    }
}
