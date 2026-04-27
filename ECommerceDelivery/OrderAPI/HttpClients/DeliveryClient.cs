namespace OrderAPI.HttpClients
{
    public class DeliveryClient : IDeliveryClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<DeliveryClient> _logger;

        public DeliveryClient(HttpClient http, ILogger<DeliveryClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task CreateDeliveryAsync(int orderId)
        {
            try
            {
                // Delivery API fetches full order details itself
                var resp = await _http.PostAsync(
                    $"api/delivery/create-from-order/{orderId}", null);

                if (!resp.IsSuccessStatusCode)
                    _logger.LogWarning("Delivery creation failed for Order {Id}: {Code}",
                        orderId, resp.StatusCode);
                else
                    _logger.LogInformation("Delivery triggered for Order {Id}", orderId);
            }
            catch (Exception ex)
            {
                // Never fail the order if delivery notification fails
                _logger.LogError(ex, "DeliveryClient failed for Order {Id}", orderId);
            }
        }
    }
}
