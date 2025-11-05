using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Percistance.Interfaces
{
    public interface IOrderDbRepository
    {
        Task<GeometryData> GetOrderRequestDetails(Guid orderId);
        Task<OrderStatus> GetOrderStatusAsync(Guid orderId);
        Task UpdateOrderAsync(Guid order, OrderStatus orderStatus);
        Task<OrderType> GetOrderTypeAsync(Guid orderId);
    }
}
