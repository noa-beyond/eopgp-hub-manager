using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface IKafkaService<T>
        where T : IKafkaMessage
    {
        Task Start(CancellationToken cancellationToken);
        void Stop();
    }
}
