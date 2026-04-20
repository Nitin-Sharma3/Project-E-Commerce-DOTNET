using Ecommerce.Customer.CartAPI.DTOs;
using Ecommerce.Customer.CartAPI.Models;
using Ecommerce.Customer.CartAPI.Repositories;
namespace Ecommerce.Customer.CartAPI.Services
{
    
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartResponseDto> GetCart(string userId)
        {
            var cart = await _cartRepository.GetCartByUserId(userId);

            if (cart == null)
                return new CartResponseDto { UserId = userId, Items = new List<CartItemResponseDto>(), TotalAmount = 0 };

            return new CartResponseDto
            {
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    SubTotal = i.Price * i.Quantity,
                    Image = i.Image
                }).ToList(),
                TotalAmount = cart.TotalAmount
            };
        }

        public async Task<string> AddToCart(string userId, AddToCartDto dto)
        {
            var product = SeedProducts().FirstOrDefault(p => p.Id == dto.ProductId);
            if (product == null) return "Product not found";

            var cart = await _cartRepository.GetCartByUserId(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                await _cartRepository.AddCart(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
                existingItem.Quantity += dto.Quantity;
            else
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = dto.Quantity,
                    Image = product.Image
                });

            cart.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

            await _cartRepository.SaveChanges();

            return "Item added";
        }

        public async Task<string> UpdateCart(string userId, UpdateCartDto dto)
        {
            var cart = await _cartRepository.GetCartByUserId(userId);
            if (cart == null) return "Cart not found";

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null) return "Item not found";

            item.Quantity = dto.Quantity;

            cart.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

            await _cartRepository.SaveChanges();

            return "Cart updated";
        }

        public async Task<string> RemoveItem(string userId, string productId)
        {
            var cart = await _cartRepository.GetCartByUserId(userId);
            if (cart == null) return "Cart not found";

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return "Item not found";

            cart.Items.Remove(item);

            cart.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

            await _cartRepository.SaveChanges();

            return "Item removed";
        }

        public async Task<string> ClearCart(string userId)
        {
            var cart = await _cartRepository.GetCartByUserId(userId);
            if (cart == null) return "Cart not found";

            cart.Items.Clear();
            cart.TotalAmount = 0;

            await _cartRepository.SaveChanges();

            return "Cart cleared";
        }

        // TEMP product seed
        private List<Product> SeedProducts()
        {
            return new List<Product>
        {
            new Product { Id = "p101", Name = "Wireless Mouse", Price = 799, Image = "mouse.jpg" },
            new Product { Id = "p102", Name = "Bluetooth Headphones", Price = 1999, Image = "headphones.jpg" },
            new Product { Id = "p10", Name = "Test Product", Price = 500 ,Image = "headphones.jpg"}
            };
        }
    }
}
