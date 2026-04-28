using Reviews.DTOs;

namespace Reviews.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;

        public OrderService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> HasUserPurchasedProduct(int userId, int productId)
        {
            var orders = await _http.GetFromJsonAsync<List<OrderDTO>>($"api/orders/user/{userId}");

            if (orders == null) return false;

            return orders.Any(order =>
                order.Status == "Delivered" &&
                order.OrderItems.Any(item => item.ProductId == productId)
            );
        }
    }

}
