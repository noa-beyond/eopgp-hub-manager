using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.ValueObjects.ChDM
{
    public class ChDetectionMappingProducerMessage : IKafkaMessage
    {
        [JsonProperty("orderId")]
        public Guid OrderId { get; set; }
        [JsonProperty("initialSelectionProductPaths")]
        public List<string> InitialSelectionProductPaths { get; set; }
        [JsonProperty("finalSelectionProductPaths")]
        public List<string> FinalSelectionProductPaths { get; set; }
        [JsonProperty("geometry")]
        public GeometryData Geometry { get; set; }
    }
}