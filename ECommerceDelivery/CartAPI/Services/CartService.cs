    using CartAPI.DTOs;
    using CartAPI.Models;
    using CartAPI.Repositories.Interfaces;
   
   namespace CartAPI.Services
{


    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        // ── Helpers ──────────────────────────────────────────

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            return await _cartRepository.GetCartByUserIdAsync(userId)
                   ?? await _cartRepository.CreateCartAsync(userId);
        }

        private static CartDto MapToDto(Cart cart) => new()
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice
            }).ToList()
        };

        // ── Public API ───────────────────────────────────────

        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddItemAsync(int userId, AddCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (dto.UnitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative.");

            var cart = await GetOrCreateCartAsync(userId);

            // If product already in cart → increase quantity
            var existing = await _cartRepository.GetCartItemAsync(cart.Id, dto.ProductId);
            if (existing is not null)
            {
                existing.Quantity += dto.Quantity;
                await _cartRepository.UpdateCartItemAsync(existing);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice
                };
                await _cartRepository.AddCartItemAsync(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangesAsync();

            // Reload to return fresh state
            var updated = (await _cartRepository.GetCartByUserIdAsync(userId))!;
            return MapToDto(updated);
        }

        public async Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var item = await _cartRepository.GetCartItemByIdAsync(cartItemId)
                       ?? throw new KeyNotFoundException("Cart item not found.");

            if (item.Cart.UserId != userId)
                throw new UnauthorizedAccessException("Cart item does not belong to this user.");

            item.Quantity = dto.Quantity;
            await _cartRepository.UpdateCartItemAsync(item);

            item.Cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangesAsync();

            var updated = (await _cartRepository.GetCartByUserIdAsync(userId))!;
            return MapToDto(updated);
        }

        public async Task<CartDto> RemoveItemAsync(int userId, int cartItemId)
        {
            var item = await _cartRepository.GetCartItemByIdAsync(cartItemId)
                       ?? throw new KeyNotFoundException("Cart item not found.");

            if (item.Cart.UserId != userId)
                throw new UnauthorizedAccessException("Cart item does not belong to this user.");

            await _cartRepository.RemoveCartItemAsync(item);

            var updated = (await _cartRepository.GetCartByUserIdAsync(userId))!;
            return MapToDto(updated);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId)
                       ?? throw new KeyNotFoundException("Cart not found for this user.");

            await _cartRepository.ClearCartAsync(cart.Id);
        }
    }
}
