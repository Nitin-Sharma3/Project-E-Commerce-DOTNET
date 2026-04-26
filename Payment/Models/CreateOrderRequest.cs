namespace RazorpayApi.Models
{
    public class CreateOrderRequest
    {
        public int? UserId { get; set; }
        public int? OrderId { get; set; }
        public int? AddressId { get; set; }
        public string? ExternalOrderId { get; set; }
        public int Amount { get; set; } = 100;
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public decimal TaxRate { get; set; } = 0m;
        public decimal DiscountAmount { get; set; } = 0m;
        public SellerDetails? SellerDetails { get; set; }
        public CustomerDetails? CustomerDetails { get; set; }
    }
}
