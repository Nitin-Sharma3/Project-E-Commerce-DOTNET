using DeliveryService.DTOs;

namespace DeliveryService.HttpClients
{
    public interface IOrderClient
    {
        Task<OrderApiResponse?> GetOrderAsync(int userId, int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}
