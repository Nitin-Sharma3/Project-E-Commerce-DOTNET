using CartAPI.DTOs;
    
namespace CartAPI.Services
{
    
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task<CartDto> AddItemAsync(int userId, AddCartItemDto dto);
        Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
        Task<CartDto> RemoveItemAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
    }
}
