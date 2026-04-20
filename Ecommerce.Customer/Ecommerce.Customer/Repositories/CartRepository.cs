   using Microsoft.EntityFrameworkCore;
    using Ecommerce.Customer.CartAPI.Data;
    using Ecommerce.Customer.CartAPI.Models;
namespace Ecommerce.Customer.CartAPI.Repositories
{
 
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceCustomerCartAPIContext _context;

        public CartRepository(EcommerceCustomerCartAPIContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByUserId(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddCart(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
