using Ecommerce.Customer.WishlistAPI.DTOs;
using Ecommerce.Customer.WishlistAPI.Models;
using Ecommerce.Customer.WishlistAPI.Repositories;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Ecommerce.Customer.WishlistAPI.Services
{
    public class WishlistService
    {
        private readonly IWishlistRepository _repo;
        private readonly ICartApiClient _cartApiClient;

        public WishlistService(IWishlistRepository repo, ICartApiClient cartService)
        {
            _repo = repo;
            _cartApiClient = cartService;
        }

        public async Task AddToWishlist(string userId, string productId)
        {
            var exists = await _repo.Exists(userId, productId);

            if (exists)
                return; // avoid duplicates

            var item = new WishlistEntity
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.Add(item);
        }
        public async Task MoveToCart(string userId, string productId)
        {
            Console.WriteLine($"UserId: {userId}, ProductId: {productId}");

            var item = await _repo.GetItem(userId, productId);

            Console.WriteLine(item == null ? "Item NOT FOUND" : "Item FOUND");


            var success = await _cartApiClient.AddToCart(userId, productId, 1);

            if (!success)
            {
                Console.WriteLine("Cart API failed");
                return;
            }

            Console.WriteLine("Calling Remove...");
            await _repo.Remove(productId, userId);
            Console.WriteLine("Remove called");
        }
        public async Task RemoveFromWishlist(string userId, string productId)
        {
            await _repo.Remove(productId, userId);
        }

        public async Task<List<WishlistResponseDto>> GetWishlist(string userId)
        {
            var items = await _repo.GetByUserId(userId);

            return items.Select(x => new WishlistResponseDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                ImageUrl = x.ImageUrl
            }).ToList();
        }
    }
}
