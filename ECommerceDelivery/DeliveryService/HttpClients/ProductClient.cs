using DeliveryService.DTOs;
using System.Text.Json;

namespace DeliveryService.HttpClients
{
    public class ProductClient(HttpClient http, ILogger<ProductClient> logger) : IProductClient
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // ProductAPI: GET api/product/{id}
        public async Task<ProductApiResponse?> GetProductAsync(int productId)
        {
            try
            {
                var resp = await http.GetAsync($"api/product/{productId}");
                if (!resp.IsSuccessStatusCode) return null;
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductApiResponse>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetProduct failed for ProductId {Id}", productId);
                return null;
            }
        }
    }
}
