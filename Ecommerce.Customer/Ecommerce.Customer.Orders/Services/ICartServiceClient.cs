using Ecommerce.Customer.CartAPI.DTOs;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public interface ICartServiceClient
    {
        Task<CartResponseDto?> GetCart(string userId);
        Task ClearCart(string userId);
    }
}