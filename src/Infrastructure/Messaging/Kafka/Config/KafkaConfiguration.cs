using Infrastructure.Kafka.Config;

namespace Infrastructure.Messaging.Kafka.Config
{
    public class KafkaConfiguration
    {
        public GatewayKafkaConfiguration GatewayKafkaConfiguration { get; set; }
        public HarvesterConsumerKafkaConfiguration HarvesterConsumerKafkaConfiguration { get; set; }
        public HarvesterProducerKafkaConfiguration HarvesterProducerKafkaConfiguration { get; set; }
        public StacConsumerKafkaConfiguration StacConsumerKafkaConfiguration { get; set; }
        public StacProducerKafkaConfiguration StacProducerKafkaConfiguration { get; set; }
        public ChDetectionMappingConsumerKafkaConfiguration ChDetectionMappingConsumerKafkaConfiguration { get; set; }
        public ChDetectionMappingProducerKafkaConfiguration ChDetectionMappingProducerKafkaConfiguration { get; set; }

    }
}
