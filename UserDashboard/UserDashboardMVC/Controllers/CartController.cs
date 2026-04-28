using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using UserDashboardMVC.Models;
using UserDashboardMVC.ViewModels;

namespace UserDashboardMVC.Controllers
{
   

    public class CartController : Controller
    {
        private readonly HttpClient _cartClient;
        private readonly HttpClient _productClient;
        private readonly ILogger<CartController> _logger;
        private const int CurrentUserId = 1;

        public CartController(IHttpClientFactory factory, ILogger<CartController> logger)
        {
            _cartClient = factory.CreateClient("CartAPI");
            _productClient = factory.CreateClient("ProductAPI");
            _logger = logger;
        }

        // GET /Cart
        public async Task<IActionResult> Index()
        {
            var model = new CartPageViewModel { UserId = CurrentUserId };
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // 1. Fetch cart from CartAPI
                var cartResp = await _cartClient.GetAsync($"api/users/{CurrentUserId}/cart");
                if (cartResp.IsSuccessStatusCode)
                {
                    var json = await cartResp.Content.ReadAsStringAsync();
                    var cart = JsonSerializer.Deserialize<CartApiResponse>(json, options);
                    if (cart?.Items != null)
                    {
                        model.Items = cart.Items.Select(i => new CartItemViewModel
                        {
                            Id = i.Id,
                            ProductId = i.ProductId,
                            ProductName = $"Product #{i.ProductId}",  // swap with ProductAPI name
                            Variant = "Default",
                            ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=300",
                            UnitPrice = i.UnitPrice,
                            Quantity = i.Quantity
                        }).ToList();
                    }
                }

                // 2. Fetch suggested products
                var prodResp = await _productClient.GetAsync("api/products");
                if (prodResp.IsSuccessStatusCode)
                {
                    var json = await prodResp.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? new();
                    model.SuggestedProducts = products.Take(3).ToList();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to load cart"); }

            return View(model);
        }

        // POST /Cart/UpdateQuantity
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    // quantity = 0 means remove
                    await _cartClient.DeleteAsync($"api/users/{CurrentUserId}/cart/items/{cartItemId}");
                }
                else
                {
                    var payload = new { quantity };
                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    await _cartClient.PutAsync($"api/users/{CurrentUserId}/cart/items/{cartItemId}", content);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to update cart item {Id}", cartItemId); }

            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Remove
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            try
            {
                await _cartClient.DeleteAsync($"api/users/{CurrentUserId}/cart/items/{cartItemId}");
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to remove cart item {Id}", cartItemId); }

            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try { await _cartClient.DeleteAsync($"api/users/{CurrentUserId}/cart"); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to clear cart"); }
            return RedirectToAction(nameof(Index));
        }

        // ── Internal response shapes ──────────────────────────
        private class CartApiResponse
        {
            public int Id { get; set; }
            public List<CartApiItem> Items { get; set; } = new();
            public decimal TotalAmount { get; set; }
        }
        private class CartApiItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }

}
