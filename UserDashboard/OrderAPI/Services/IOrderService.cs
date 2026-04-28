using OrderAPI.DTOs;

namespace OrderAPI.Services;

public interface IOrderService
{
    Task<OrderResponseDto> PlaceOrderAsync(int userId, PlaceOrderDto dto);
    Task<OrderResponseDto> GetOrderAsync(int userId, int orderId);
    Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId);
    Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
    Task<OrderResponseDto> CancelOrderAsync(int userId, int orderId);
}
