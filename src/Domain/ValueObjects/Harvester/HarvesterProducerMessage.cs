using Domain.Interfaces;

namespace Domain.ValueObjects.Harvester
{

    public class HarvesterProducerMessage : IKafkaMessage
    {
        //[JsonProperty("ids")]
        public List<Guid> Ids { get; set; }
    }
}