using Ecommerce.Customer.CartAPI.DTOs;

namespace Ecommerce.Customer.CartAPI.Services
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCart(string userId);
        Task<string> AddToCart(string userId, AddToCartDto dto);
        Task<string> UpdateCart(string userId, UpdateCartDto dto);
        Task<string> RemoveItem(string userId, string productId);
        Task<string> ClearCart(string userId);
    }
}
