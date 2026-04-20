using System.Threading.Tasks;

namespace Ecommerce.Customer.WishlistAPI.Services
{
    public interface ICartApiClient
    {
        Task<bool> AddToCart(string userId, string productId, int quantity);
    }
}
