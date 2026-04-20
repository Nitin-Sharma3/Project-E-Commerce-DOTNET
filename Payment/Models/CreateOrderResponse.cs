namespace RazorpayApi.Models
{
    public class CreateOrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
    }
}
