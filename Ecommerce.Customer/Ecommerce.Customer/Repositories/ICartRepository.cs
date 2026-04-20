using Ecommerce.Customer.CartAPI.Models;

namespace Ecommerce.Customer.CartAPI.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByUserId(string userId);
        Task AddCart(Cart cart);
        Task SaveChanges();
    }
}
