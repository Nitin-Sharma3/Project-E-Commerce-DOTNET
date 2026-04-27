using DeliveryService.DTOs;

namespace DeliveryService.HttpClients
{
    public interface IProductClient
    {
        Task<ProductApiResponse?> GetProductAsync(int productId);
    }
}
