using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Customer.WishlistAPI.Data;
using Ecommerce.Customer.WishlistAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Customer.WishlistAPI.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly EcommerceCustomerWishlistAPIContext _context;

        public WishlistRepository(EcommerceCustomerWishlistAPIContext context)
        {
            _context = context;
        }   

        public async Task<List<WishlistEntity>> GetByUserId(string userId)
        {
            return await _context.WishlistEntity
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<WishlistEntity> GetItem(string userId, string productId)
        {
            return await _context.WishlistEntity
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ProductId.ToLower() == productId.ToLower());
        }

        public async Task<bool> Exists(string userId, string productId)
        {
            return await _context.WishlistEntity
                .AnyAsync(x => x.UserId == userId && x.ProductId == productId);
        }

        public async Task Add(WishlistEntity item)
        {
            await _context.WishlistEntity.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(string productId, string userId)
        {
            Console.WriteLine($"Remove called with: {productId}, {userId}");

            var items = await _context.WishlistEntity
                .Where(x => x.UserId == userId)
                .ToListAsync();

            Console.WriteLine($"Items for user: {items.Count}");

            var item = items.FirstOrDefault(x =>
                x.ProductId.ToLower() == productId.ToLower());

            Console.WriteLine(item == null ? "ITEM NOT FOUND ❌" : "ITEM FOUND ✅");

            if (item != null)
            {
                _context.WishlistEntity.Remove(item);
                await _context.SaveChangesAsync();
                Console.WriteLine("ITEM REMOVED ✅");
            }
        }
    }
}
