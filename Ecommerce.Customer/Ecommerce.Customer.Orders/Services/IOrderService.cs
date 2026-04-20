using Ecommerce.Customer.OrderAPI.DTOs;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> PlaceOrder(string userId, PlaceOrderDto dto);
        Task<List<OrderListDto>> GetOrders(string userId);
        Task<OrderResponseDto?> GetOrderById(string userId, int orderId);
        Task<OrderResponseDto?> GetOrderByNumber(string userId, string orderNumber);
        Task<OrderResponseDto?> UpdateStatus(int orderId, UpdateOrderStatusDto dto);
        Task<bool> CancelOrder(string userId, int orderId, string? reason);
    }
}