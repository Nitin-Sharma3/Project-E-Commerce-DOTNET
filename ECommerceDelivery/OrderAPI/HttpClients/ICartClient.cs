 using OrderAPI.DTOs;
namespace OrderAPI.HttpClients
{
    public interface ICartClient
    {
        Task<CartDto?> GetCartAsync(int userId);
        Task ClearCartAsync(int userId);
    }
}
