using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Customer.WishlistAPI.Models;

namespace Ecommerce.Customer.WishlistAPI.Repositories
{
    public interface IWishlistRepository
    {
        Task<List<WishlistEntity>> GetByUserId(string userId);

        Task Add(WishlistEntity item);
        Task<WishlistEntity?> GetItem(string userId, string productId);
        Task Remove(string productId, string userId);

        Task<bool> Exists(string userId, string productId);
    }
}
