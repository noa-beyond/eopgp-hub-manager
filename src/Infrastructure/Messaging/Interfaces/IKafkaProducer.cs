using Domain.Interfaces;

namespace Infrastructure.Messaging.Interfaces
{
    public interface IKafkaProducer<T>
        where T
        : IKafkaMessage
    {
        Task ProduceAsync(T message, CancellationToken cancellationToken = default);
    }
}