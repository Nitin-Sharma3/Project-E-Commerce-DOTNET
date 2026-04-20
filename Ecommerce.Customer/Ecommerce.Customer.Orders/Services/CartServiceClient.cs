using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Ecommerce.Customer.CartAPI.DTOs;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public class CartServiceClient : ICartServiceClient
    {
        private readonly HttpClient _httpClient;

        public CartServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CartResponseDto?> GetCart(string userId)
        {
            // GET /api/cart/{userId}
            var response = await _httpClient.GetAsync($"/api/cart/{Uri.EscapeDataString(userId)}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CartResponseDto?>();
        }

        public async Task ClearCart(string userId)
        {
            // DELETE /api/cart/{userId}
            var response = await _httpClient.DeleteAsync($"/api/cart/{Uri.EscapeDataString(userId)}");
            response.EnsureSuccessStatusCode();
        }
    }
}
