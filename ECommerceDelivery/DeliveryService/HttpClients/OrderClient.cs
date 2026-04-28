using DeliveryService.DTOs;
using System.Text.Json;

namespace DeliveryService.HttpClients
{
    public class OrderClient(HttpClient http, ILogger<OrderClient> logger) : IOrderClient
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // OrderAPI: GET api/users/{userId}/orders/{orderId}
        public async Task<OrderApiResponse?> GetOrderAsync(int userId, int orderId)
        {
            try
            {
                var resp = await http.GetAsync($"api/users/{userId}/orders/{orderId}");
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("OrderAPI returned {Code} for Order {Id}", resp.StatusCode, orderId);
                    return null;
                }
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderApiResponse>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetOrder failed for OrderId {Id}", orderId);
                return null;
            }
        }

        // OrderAPI: PATCH api/orders/{orderId}/status
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var payload = new OrderStatusUpdatePayload { Status = status };
                var resp = await http.PatchAsJsonAsync(
                    $"api/orders/{orderId}/status", payload);

                if (!resp.IsSuccessStatusCode)
                    logger.LogWarning("OrderAPI status update failed: {Code}", resp.StatusCode);

                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UpdateOrderStatus failed for Order {Id}", orderId);
                return false;
            }
        }
    }
}
