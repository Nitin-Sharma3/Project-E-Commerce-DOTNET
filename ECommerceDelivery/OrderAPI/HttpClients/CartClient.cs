using OrderAPI.DTOs;
    using OrderAPI.HttpClients;
namespace OrderAPI.HttpClients
{
    public class CartClient : ICartClient
    {
        private readonly HttpClient _httpClient;

        public CartClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CartDto?> GetCartAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}/cart");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CartDto>();
        }

        public async Task ClearCartAsync(int userId)
        {
            await _httpClient.DeleteAsync($"api/users/{userId}/cart");
        }
    }
}
