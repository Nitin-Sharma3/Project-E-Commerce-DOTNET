namespace RazorpayApi.Models
{
    public class CreateOrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string? ExternalOrderId { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
    }
}
