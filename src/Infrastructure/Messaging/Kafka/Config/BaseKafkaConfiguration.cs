using Confluent.Kafka;

namespace Infrastructure.Kafka.Config
{
    public class BaseKafkaConfiguration
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string Topic { get; set; }
        public AutoOffsetReset AutoOffsetReset { get; set; }
        public bool EnableAutoCommit { get; set; }
        public int SessionTimeoutMs { get; set; }
    }
    public class GatewayKafkaConfiguration : BaseKafkaConfiguration;
    public class HarvesterConsumerKafkaConfiguration : BaseKafkaConfiguration;
    public class HarvesterProducerKafkaConfiguration : BaseKafkaConfiguration;
    public class StacConsumerKafkaConfiguration : BaseKafkaConfiguration;
    public class StacProducerKafkaConfiguration : BaseKafkaConfiguration;
    public class ChDetectionMappingKafkaConfiguration : BaseKafkaConfiguration;
    public class ChDetectionMappingConsumerKafkaConfiguration : BaseKafkaConfiguration;
    public class ChDetectionMappingProducerKafkaConfiguration : BaseKafkaConfiguration;
}