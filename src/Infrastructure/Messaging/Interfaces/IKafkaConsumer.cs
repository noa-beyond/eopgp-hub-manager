using Domain.Interfaces;

namespace Infrastructure.Messaging.Interfaces
{
    public interface IKafkaConsumer<T>
        where T
        : IKafkaMessage
    {
        Task Init();
        void Stop();
    }
}