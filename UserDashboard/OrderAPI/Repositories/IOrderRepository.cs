using OrderAPI.Models;

namespace OrderAPI.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId);
    Task<Order?> GetByIdAndUserAsync(int orderId, int userId);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<bool> ExistsAsync(int orderId);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task<int> CountByUserAsync(int userId);
}
