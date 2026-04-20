namespace Ecommerce.Customer.WishlistAPI.Services
{
    using System.Net.Http.Json;

    public class CartApiClient : ICartApiClient
    {
        private readonly HttpClient _httpClient;

        public CartApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> AddToCart(string userId, string productId, int quantity)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Cart/add", new
            {
                userId,
                productId,
                quantity
            });

            Console.WriteLine($"Cart API Status: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
    }
}
