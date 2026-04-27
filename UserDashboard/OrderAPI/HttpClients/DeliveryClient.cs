namespace OrderAPI.HttpClients
{
    public class DeliveryClient(HttpClient http, ILogger<DeliveryClient> logger)
    : IDeliveryClient
    {
        public async Task CreateDeliveryAsync(int orderId, int userId)
        {
            try
            {
                // POST api/delivery/create-from-order
                var resp = await http.PostAsJsonAsync(
                    "api/delivery/create-from-order",
                    new { OrderId = orderId, UserId = userId });

                if (!resp.IsSuccessStatusCode)
                    logger.LogWarning("Delivery creation failed for Order {Id}: {Code}",
                        orderId, resp.StatusCode);
                else
                    logger.LogInformation("Delivery created for Order {Id}", orderId);
            }
            catch (Exception ex)
            {
                // Never fail the order
                logger.LogError(ex, "DeliveryClient failed for Order {Id}", orderId);
            }
        }
    }
}
