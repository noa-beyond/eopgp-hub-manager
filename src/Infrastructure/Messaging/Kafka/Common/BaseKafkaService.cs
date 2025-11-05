using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Messaging.Interfaces;

namespace Infrastructure.Messaging.Kafka.Common
{
    public abstract class BaseKafkaService<T>(IKafkaConsumer<T> kafkaConsumer)
        : IKafkaService<T>
        where T : IKafkaMessage
    {
        private readonly IKafkaConsumer<T> _kafkaConsumer = kafkaConsumer;

        public async Task Start(CancellationToken cancellationToken)
        {
            await _kafkaConsumer.Init();
        }

        public void Stop()
        {
        }
    }
}