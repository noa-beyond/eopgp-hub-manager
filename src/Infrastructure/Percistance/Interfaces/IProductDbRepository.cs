using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Percistance.Interfaces
{
    public interface IProductDbRepository
    {
        Task<List<Product>> GetProductsByIdAsync(List<Guid> ids);
        Task<Product> GetProductByIdAsync(Guid productId);
        Task<List<Product>> GetProductsByOrderIdAndGroupIdAsync(Guid ids, GroupType groupType);
        Task<bool> DeleteProductAsync(Product product);
        Task<bool> UpdateProductAysnc(Guid orderId);
    }
}
