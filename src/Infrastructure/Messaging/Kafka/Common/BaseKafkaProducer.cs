using Confluent.Kafka;
using Domain.Interfaces;
using Infrastructure.Kafka.Config;
using Infrastructure.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Messaging.Kafka.Common
{
    public abstract class BaseKafkaProducer<T>(BaseKafkaConfiguration kafkaConfiguration, ILogger<BaseKafkaProducer<T>> logger)
        : IKafkaProducer<T>
        where T : IKafkaMessage
    {
        private readonly BaseKafkaConfiguration _kafkaSettings = kafkaConfiguration;
        public async Task ProduceAsync(T message, CancellationToken cancellationToken = default)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
            };

            using IProducer<Null, string> producer = new ProducerBuilder<Null, string>(config).Build();
            var jsonValue = JsonConvert.SerializeObject(message);

            var messageS = new Message<Null, string>
            {
                Value = jsonValue
            };
            try
            {


                var deliveryResult = await producer.ProduceAsync(_kafkaSettings.Topic, messageS);

                logger.LogInformation("Message delivered to {Topic} | With message: {Message}", deliveryResult.Topic, messageS.Value);
            }
            catch (ProduceException<string, string> e)
            {
                logger.LogError(e, "Failed to produce message");
                logger.Log(
                    LogLevel.Error,
                    new EventId(0, "ProducerConnection"),
                    $"Message failed to be delivered to {_kafkaSettings.Topic} | Message: {messageS.Value}",
                    e,
                    (state, success) => state
                );
            }
        }
    }
}
